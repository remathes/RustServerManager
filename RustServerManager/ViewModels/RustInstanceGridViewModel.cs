using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Geo;
using MaterialDesignThemes.Wpf;
using Microsoft.Xaml.Behaviors.Media;
using MySql.Data.MySqlClient;
using RustServerManager.Controls;
using RustServerManager.Services;
using RustServerManager.Utils;
using RustServerManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using static Mysqlx.Notice.Warning.Types;

namespace RustServerManager.ViewModels
{
    public partial class RustInstanceGridViewModel : ObservableObject, INotifyPropertyChanged
    {
        private RconClient _rconClient;
        public string StartDialogMessage { get; set; }
        public bool IsStartDialogOpen { get; set; }

        [ObservableProperty]
        private RustInstanceGridItemViewModel? selectedInstance;

        public UserControl SelectedPage => SelectedInstance?.CurrentPage;

        partial void OnSelectedInstanceChanged(RustInstanceGridItemViewModel value)
        {
            OnPropertyChanged(nameof(SelectedPage));
        }

        public ObservableCollection<RustInstanceGridItemViewModel> Instances { get; set; } = new();

        public async Task LoadInstances()
        {

            Instances.Clear();

            using var conn = new MySqlConnection(DatabaseHelper.GetConnectionString());
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Instances", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // Safely process the MapName
                string rawMap = reader["MapName"] as string ?? "";
                string safeMapName = rawMap;
                if (rawMap.Contains('.') && rawMap.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = rawMap.Split('.');
                    if (parts.Length >= 2)
                        safeMapName = parts[0] + parts[1].Replace(".map", "", StringComparison.OrdinalIgnoreCase);
                }

                var instance = new RustServerInstance
                {
                    Identity = reader["Identity"].ToString(),
                    Description = reader["Description"].ToString(),
                    ServerHostname = reader["ServerHostname"].ToString(),
                    ServerUrl = reader["ServerUrl"].ToString(),
                    ServerIp = reader["ServerIp"].ToString(),
                    ServerPort = Convert.ToInt32(reader["ServerPort"]),
                    RconIp = reader["RconIp"].ToString(),
                    RconPort = Convert.ToInt32(reader["RconPort"]),
                    RconPassword = reader["RconPassword"].ToString(),
                    RconWeb = Convert.ToInt32(reader["RconWeb"]),
                    MaxPlayers = Convert.ToInt32(reader["MaxPlayers"]),
                    ServerTickrate = Convert.ToInt32(reader["ServerTickrate"]),
                    ServerSaveInterval = Convert.ToInt32(reader["ServerSaveInterval"]),
                    UseCustomMap = Convert.ToBoolean(reader["UseCustomMap"]),
                    Seed = Convert.ToInt32(reader["Seed"]),
                    WorldSize = Convert.ToInt32(reader["WorldSize"]),
                    Level = reader["Level"].ToString(),
                    MapName = safeMapName,
                    ServerLevelUrl = reader["ServerLevelUrl"].ToString(),
                    SteamCmdPath = reader["SteamCmdPath"].ToString(),
                    InstallDirectory = reader["InstallDirectory"].ToString(),
                    RustDedicatedProcess = reader["RustDedicatedProcess"].ToString(),
                    ServerCfg = reader["ServerCfg"].ToString(),
                    MySqlHost = reader["MySqlHost"].ToString(),
                    MySqlPort = Convert.ToInt32(reader["MySqlPort"]),
                    MySqlUsername = reader["MySqlUsername"].ToString(),
                    MySqlPassword = reader["MySqlPassword"].ToString(),
                    MySqlDatabaseName = reader["MySqlDatabaseName"].ToString(),
                    AutoStart = Convert.ToBoolean(reader["AutoStart"]),
                    AutoUpdate = Convert.ToBoolean(reader["AutoUpdate"]),
                    LastWiped = reader.IsDBNull(reader.GetOrdinal("LastWiped"))
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("LastWiped")),
                    ProcessId = reader.IsDBNull(reader.GetOrdinal("ProcessId"))
                        ? 0
                        : reader.GetInt32(reader.GetOrdinal("ProcessId")),
                };

                Instances.Add(new RustInstanceGridItemViewModel(instance));
            }

            SelectedInstance = Instances.First();

            // ✅ Start metrics monitoring for the first instance
            SelectedInstance.StartMetricsUpdate(SelectedInstance.Instance);
        }

        public IRelayCommand NewCommand { get; }
        public IRelayCommand StartCommand { get; }
        public IAsyncRelayCommand StopCommand { get; }
        public IRelayCommand EditCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IRelayCommand BackInstanceCommand { get; }
        public IRelayCommand NextInstanceCommand { get; }
        public ICommand ConnectToConsoleCommand { get; }

        public RustInstanceGridViewModel()
        {
            NewCommand = new RelayCommand(OpenNewDialog);
            StartCommand = new RelayCommand(StartInstance);
            StopCommand = new AsyncRelayCommand(StopInstance);
            DeleteCommand = new AsyncRelayCommand(DeleteInstance);
            EditCommand = new RelayCommand(OpenEditDialog, () => SelectedInstance != null);
            BackInstanceCommand = new RelayCommand(GoBackInstance, CanGoBackInstance);
            NextInstanceCommand = new RelayCommand(GoNextInstance, CanGoNextInstance);
            ConnectToConsoleCommand = new RelayCommand(ExecuteConnectToConsole);
        }

        private void GoBackInstance()
        {
            if (Instances == null || SelectedInstance == null) return;

            var currentIndex = Instances.IndexOf(SelectedInstance);
            if (currentIndex > 0)
            {
                SelectedInstance.StopMetricsUpdate(); // 👈 stop monitoring old instance

                SelectedInstance = Instances[currentIndex - 1];

                SelectedInstance.StartMetricsUpdate(SelectedInstance.Instance); // 👈 start new monitoring

                BackInstanceCommand.NotifyCanExecuteChanged();
                NextInstanceCommand.NotifyCanExecuteChanged();
                if(_rconClient != null)
                    _rconClient = new RconClient(SelectedInstance.Identity);
            }
        }

        private void GoNextInstance()
        {
            if (Instances == null || SelectedInstance == null) return;

            var currentIndex = Instances.IndexOf(SelectedInstance);
            if (currentIndex < Instances.Count - 1)
            {
                SelectedInstance.StopMetricsUpdate(); // 👈 stop old

                SelectedInstance = Instances[currentIndex + 1];

                SelectedInstance.StartMetricsUpdate(SelectedInstance.Instance); // 👈 start new

                BackInstanceCommand.NotifyCanExecuteChanged();
                NextInstanceCommand.NotifyCanExecuteChanged();
                if(_rconClient != null)
                    _rconClient = new RconClient(SelectedInstance.Identity);
            }
        }

        private bool CanGoBackInstance()
        {
            if (Instances == null || SelectedInstance == null) return false;

            return Instances.IndexOf(SelectedInstance) > 0;

        }

        private bool CanGoNextInstance()
        {
            if (Instances == null || SelectedInstance == null) return false;
            return Instances.IndexOf(SelectedInstance) < Instances.Count - 1;
        }

        private Process? _myLaunchedProcess;

        private async void StartInstance()
        {
            if (SelectedInstance is not RustInstanceGridItemViewModel selected)
                return;

            string rustDedicatedPath = Path.Combine(selected.Instance.InstallDirectory, "RustDedicated.exe");

            if(File.Exists(Path.Combine(selected.Instance.InstallDirectory, "RustUpdate.exe")))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.Combine(selected.Instance.InstallDirectory, "RustUpdate.exe");
                psi.Arguments = $"{selected.Instance.SteamCmdPath} {selected.Instance.InstallDirectory}";
                psi.UseShellExecute = false;
                using (Process process = new Process()) 
                {
                    process.StartInfo = psi;
                    process.Start();
                    process.WaitForExit();
                }
            }

            if (!File.Exists(rustDedicatedPath))
                return;

            string logFolder = Path.Combine(
                selected.Instance.InstallDirectory,
                "server",
                selected.Instance.Identity,
                "rustserverlog"
            );

            Directory.CreateDirectory(logFolder);

            string logFilePath = Path.Combine(
                logFolder,
                $"serverlog-{DateTime.Now:MMddyyyy}.log"
            );

            var argsBuilder = new StringBuilder();

            // Process-level flags first
            argsBuilder.Append($"-batchmode ");
            argsBuilder.Append($"+server.port {selected.Instance.ServerPort} ");
            argsBuilder.Append($"+rcon.port {selected.Instance.RconPort} ");
            if (selected.Instance.AppPort > 0)
            {
                argsBuilder.Append($"+app.port {selected.Instance.AppPort} ");
            }
            if (selected.Instance.UseCustomMap && !string.IsNullOrWhiteSpace(selected.Instance.ServerLevelUrl))
            {
                argsBuilder.Append($"+server.levelurl \"{selected.Instance.ServerLevelUrl}\" ");
            }
            else
            {
                argsBuilder.Append("+server.level \"Procedural Map\" ");
                argsBuilder.Append($"+server.worldsize {selected.Instance.WorldSize} ");
                argsBuilder.Append($"+server.seed {selected.Instance.Seed} ");
            }
            argsBuilder.Append($"+server.hostname \"{selected.Instance.ServerHostname}\" ");
            argsBuilder.Append($"+server.description \"{selected.Instance.Description}\" ");
            argsBuilder.Append($"+server.url \"{selected.Instance.ServerUrl}\" ");
            argsBuilder.Append($"+server.identity \"{selected.Instance.Identity}\" ");
            argsBuilder.Append($"+rcon.password \"{selected.Instance.RconPassword}\" ");
            argsBuilder.Append($"+server.maxplayers {selected.Instance.MaxPlayers} ");
            argsBuilder.Append($"+rcon.web {selected.Instance.RconWeb} ");
            argsBuilder.Append($"+server.saveinterval {selected.Instance.ServerSaveInterval} ");
            argsBuilder.Append($"+server.tickrate {selected.Instance.ServerTickrate} ");
            argsBuilder.Append($"-logfile \"{logFilePath}\"");
            

            var startInfo = new ProcessStartInfo
            {
                FileName = "RustDedicated.exe",
                Arguments = argsBuilder.ToString(),
                WorkingDirectory = Path.GetDirectoryName(rustDedicatedPath),
                UseShellExecute = true,
            };

            try
            {
                _myLaunchedProcess = Process.Start(startInfo);

                if (_myLaunchedProcess != null)
                {
                    selected.Instance.ProcessId = _myLaunchedProcess.Id;

                    // Optional: Save updated ProcessId to database

                    await Task.Delay(3000); // Short delay for process to stabilize

                    // 🛠 NOW launch your custom RustServerConsoleWindow
                    var consoleWindow = new RustServerConsoleWindow(
                        selected.Instance.ServerIp,
                        selected.Instance.RconPort,
                        selected.Instance.RconPassword,
                        selected.Instance.Identity,
                        logFilePath);

                    consoleWindow.Title = $"{selected.Instance.ServerHostname} Console";
                    consoleWindow.Show();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start Rust server: {ex.Message}");
            }
        }

        private async Task StopInstance()
        {
            if (SelectedInstance is not RustInstanceGridItemViewModel selected)
                return;

            try
            {
                // Try RCON first
                if (_rconClient == null)
                    _rconClient = new RconClient(selected.Instance.Identity);
                bool connected = await _rconClient.EnsureConnectedAsync(
                    selected.Instance.RconIp,
                    (ushort)selected.Instance.RconPort,
                    selected.Instance.RconPassword
                );

                if (connected)
                {
                    await _rconClient.SendCommandAsync("quit");
                    _rconClient.Disconnect();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[StopInstance] Could not connect RCON, fallback to process kill.");
                    await FallbackKillProcess(selected);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StopInstance] Error using RCON: {ex.Message}");
                await FallbackKillProcess(selected);
            }
            finally
            {
                _rconClient.Reset();
            }

            // Clear the ProcessId
            selected.Instance.ProcessId = 0;

            System.Diagnostics.Debug.WriteLine("Server stopped successfully.");
        }

        private async Task FallbackKillProcess(RustInstanceGridItemViewModel selected)
        {
            if (selected.Instance.ProcessId == 0)
                return;
            try
            {
                var process = Process.GetProcessById(selected.ProcessId);

                if (!process.HasExited)
                {
                    process.Kill(true);
                    await process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FallbackKillProcess] Error: {ex.Message}");
            }
        }

        private async void OpenNewDialog()
        {
            var editVm = new RustInstanceEditViewModel { IsNew = true };
            var dialog = new RustInstanceEditDialog { DataContext = editVm };
            await editVm.SetPortsAsync();
            var result = await DialogHost.Show(dialog, "MainDialog");

            if (result is bool saved && saved)
            {
                var newInstance = new RustServerInstance();
                editVm.ApplyTo(newInstance);
                RustConfigWriter.Save(editVm, new RustInstanceGridItemViewModel(newInstance));
                try
                {
                    using var conn = new MySqlConnection(DatabaseHelper.GetConnectionString());
                    await conn.OpenAsync();

                    using var cmd = new MySqlCommand(@"INSERT INTO Instances (
        Identity, Description, ServerHostname, ServerUrl, ServerIp, ServerPort,
        RconIp, RconPort, AppPort, RconPassword, RconWeb, MaxPlayers,
        ServerTickrate, ServerSaveInterval, UseCustomMap, Seed, WorldSize,
        Level, MapName, ServerLevelUrl, SteamCmdPath, InstallDirectory,
        RustDedicatedProcess, ServerCfg, MySqlHost, MySqlPort, MySqlUsername,
        MySqlPassword, MySqlDatabaseName, AutoStart, AutoUpdate, LastWiped, ProcessId)
        VALUES (
        @Identity, @Description, @ServerHostname, @ServerUrl, @ServerIp, @ServerPort,
        @RconIp, @RconPort, @AppPort, @RconPassword, @RconWeb, @MaxPlayers,
        @ServerTickrate, @ServerSaveInterval, @UseCustomMap, @Seed, @WorldSize,
        @Level, @MapName, @ServerLevelUrl, @SteamCmdPath, @InstallDirectory,
        @RustDedicatedProcess, @ServerCfg, @MySqlHost, @MySqlPort, @MySqlUsername,
        @MySqlPassword, @MySqlDatabaseName, @AutoStart, @AutoUpdate, @LastWiped, @ProcessId);", conn);

                    cmd.Parameters.AddWithValue("@Identity", newInstance.Identity);
                    cmd.Parameters.AddWithValue("@Description", newInstance.Description);
                    cmd.Parameters.AddWithValue("@ServerHostname", newInstance.ServerHostname);
                    cmd.Parameters.AddWithValue("@ServerUrl", newInstance.ServerUrl);
                    cmd.Parameters.AddWithValue("@ServerIp", newInstance.ServerIp);
                    cmd.Parameters.AddWithValue("@ServerPort", newInstance.ServerPort);
                    cmd.Parameters.AddWithValue("@RconIp", newInstance.RconIp);
                    cmd.Parameters.AddWithValue("@RconPort", newInstance.RconPort);
                    cmd.Parameters.AddWithValue("@AppPort", newInstance.AppPort);
                    cmd.Parameters.AddWithValue("@RconPassword", newInstance.RconPassword);
                    cmd.Parameters.AddWithValue("@RconWeb", newInstance.RconWeb);
                    cmd.Parameters.AddWithValue("@MaxPlayers", newInstance.MaxPlayers);
                    cmd.Parameters.AddWithValue("@ServerTickrate", newInstance.ServerTickrate);
                    cmd.Parameters.AddWithValue("@ServerSaveInterval", newInstance.ServerSaveInterval);
                    cmd.Parameters.AddWithValue("@UseCustomMap", newInstance.UseCustomMap);
                    cmd.Parameters.AddWithValue("@Seed", newInstance.Seed);
                    cmd.Parameters.AddWithValue("@WorldSize", newInstance.WorldSize);
                    cmd.Parameters.AddWithValue("@Level", newInstance.Level);
                    cmd.Parameters.AddWithValue("@MapName", newInstance.MapName);
                    cmd.Parameters.AddWithValue("@ServerLevelUrl", newInstance.ServerLevelUrl);
                    cmd.Parameters.AddWithValue("@SteamCmdPath", newInstance.SteamCmdPath);
                    cmd.Parameters.AddWithValue("@InstallDirectory", newInstance.InstallDirectory);
                    cmd.Parameters.AddWithValue("@RustDedicatedProcess", newInstance.RustDedicatedProcess);
                    cmd.Parameters.AddWithValue("@ServerCfg", newInstance.ServerCfg);
                    cmd.Parameters.AddWithValue("@MySqlHost", newInstance.MySqlHost);
                    cmd.Parameters.AddWithValue("@MySqlPort", newInstance.MySqlPort);
                    cmd.Parameters.AddWithValue("@MySqlUsername", newInstance.MySqlUsername);
                    cmd.Parameters.AddWithValue("@MySqlPassword", newInstance.MySqlPassword);
                    cmd.Parameters.AddWithValue("@MySqlDatabaseName", newInstance.MySqlDatabaseName);
                    cmd.Parameters.AddWithValue("@AutoStart", newInstance.AutoStart);
                    cmd.Parameters.AddWithValue("@AutoUpdate", newInstance.AutoUpdate);
                    cmd.Parameters.AddWithValue("@LastWiped", newInstance.LastWiped == DateTime.MinValue ? DBNull.Value : (object)newInstance.LastWiped);
                    cmd.Parameters.AddWithValue("@ProcessId", newInstance.ProcessId);

                    await cmd.ExecuteNonQueryAsync();

                    // ✅ Add to collection after saving to DB
                    Instances.Add(new RustInstanceGridItemViewModel(newInstance));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error inserting new instance: {ex.Message}");
                }
            }
        }

        private async void OpenEditDialog()
        {
            if (SelectedInstance is not RustInstanceGridItemViewModel selected)
                return;

            var editVm = new RustInstanceEditViewModel(selected.Instance);
            var dialog = new RustInstanceEditDialog { DataContext = editVm };

            var result = await DialogHost.Show(dialog, "MainDialog");

            if (result is bool saved && saved)
            {
                RustConfigWriter.Save(editVm, selected);
                editVm.ApplyTo(selected.Instance);

                try
                {
                    using var conn = new MySqlConnection(DatabaseHelper.GetConnectionString());
                    await conn.OpenAsync();

                    using var cmd = new MySqlCommand(@"
                UPDATE Instances SET
                    Description = @Description,
                    ServerHostname = @ServerHostname,
                    ServerUrl = @ServerUrl,
                    ServerIp = @ServerIp,
                    ServerPort = @ServerPort,
                    RconIp = @RconIp,
                    RconPort = @RconPort,
                    AppPort = @AppPort,
                    RconPassword = @RconPassword,
                    RconWeb = @RconWeb,
                    MaxPlayers = @MaxPlayers,
                    ServerTickrate = @ServerTickrate,
                    ServerSaveInterval = @ServerSaveInterval,
                    UseCustomMap = @UseCustomMap,
                    Seed = @Seed,
                    WorldSize = @WorldSize,
                    Level = @Level,
                    MapName = @MapName,
                    ServerLevelUrl = @ServerLevelUrl,
                    SteamCmdPath = @SteamCmdPath,
                    InstallDirectory = @InstallDirectory,
                    RustDedicatedProcess = @RustDedicatedProcess,
                    ServerCfg = @ServerCfg,
                    MySqlHost = @MySqlHost,
                    MySqlPort = @MySqlPort,
                    MySqlUsername = @MySqlUsername,
                    MySqlPassword = @MySqlPassword,
                    MySqlDatabaseName = @MySqlDatabaseName,
                    AutoStart = @AutoStart,
                    AutoUpdate = @AutoUpdate
                WHERE Identity = @Identity;
            ", conn);

                    RustServerInstance i = selected.Instance;

                    cmd.Parameters.AddWithValue("@Description", i.Description);
                    cmd.Parameters.AddWithValue("@ServerHostname", i.ServerHostname);
                    cmd.Parameters.AddWithValue("@ServerUrl", i.ServerUrl);
                    cmd.Parameters.AddWithValue("@ServerIp", i.ServerIp);
                    cmd.Parameters.AddWithValue("@ServerPort", i.ServerPort);
                    cmd.Parameters.AddWithValue("@RconIp", i.RconIp);
                    cmd.Parameters.AddWithValue("@RconPort", i.RconPort);
                    cmd.Parameters.AddWithValue("@AppPort", i.AppPort);
                    cmd.Parameters.AddWithValue("@RconPassword", i.RconPassword);
                    cmd.Parameters.AddWithValue("@RconWeb", i.RconWeb);
                    cmd.Parameters.AddWithValue("@MaxPlayers", i.MaxPlayers);
                    cmd.Parameters.AddWithValue("@ServerTickrate", i.ServerTickrate);
                    cmd.Parameters.AddWithValue("@ServerSaveInterval", i.ServerSaveInterval);
                    cmd.Parameters.AddWithValue("@UseCustomMap", i.UseCustomMap);
                    cmd.Parameters.AddWithValue("@Seed", i.Seed);
                    cmd.Parameters.AddWithValue("@WorldSize", i.WorldSize);
                    cmd.Parameters.AddWithValue("@Level", i.Level);
                    cmd.Parameters.AddWithValue("@MapName", i.MapName);
                    cmd.Parameters.AddWithValue("@ServerLevelUrl", i.ServerLevelUrl);
                    cmd.Parameters.AddWithValue("@SteamCmdPath", i.SteamCmdPath);
                    cmd.Parameters.AddWithValue("@InstallDirectory", i.InstallDirectory);
                    cmd.Parameters.AddWithValue("@RustDedicatedProcess", i.RustDedicatedProcess);
                    cmd.Parameters.AddWithValue("@ServerCfg", i.ServerCfg);
                    cmd.Parameters.AddWithValue("@MySqlHost", i.MySqlHost);
                    cmd.Parameters.AddWithValue("@MySqlPort", i.MySqlPort);
                    cmd.Parameters.AddWithValue("@MySqlUsername", i.MySqlUsername);
                    cmd.Parameters.AddWithValue("@MySqlPassword", i.MySqlPassword);
                    cmd.Parameters.AddWithValue("@MySqlDatabaseName", i.MySqlDatabaseName);
                    cmd.Parameters.AddWithValue("@AutoStart", i.AutoStart);
                    cmd.Parameters.AddWithValue("@AutoUpdate", i.AutoUpdate);
                    cmd.Parameters.AddWithValue("@Identity", i.Identity);

                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EditDialog] Failed to update instance in DB: {ex.Message}");
                }
                finally
                {
                    selected.RefreshFromInstance();
                }
            }
        }

        private async Task DeleteInstance()
        {
            if (SelectedInstance is not RustInstanceGridItemViewModel selected)
                return;
            if (MessageBox.Show($"Are you sure you want to delete this inatance\r\nHost Name: {selected.ServerHostname}\r\nDescription: {selected.Description}",
                "Confirm delete", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                try
                {
                    using var conn = new MySqlConnection(DatabaseHelper.GetConnectionString());
                    await conn.OpenAsync();

                    using var cmd = new MySqlCommand("DELETE FROM Instances WHERE Identity = @Identity", conn);
                    cmd.Parameters.AddWithValue("@Identity", selected.Instance.Identity);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows > 0)
                    {
                        // Successfully deleted from DB
                        Instances.Remove(selected);

                        // Clear selection after delete
                        SelectedInstance = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("DeleteInstance: No rows affected. Identity might not exist?");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DeleteInstance Error: {ex.Message}");
                }
            }
        }

        public static void SaveAllInstances(IEnumerable<RustInstanceGridViewModel> instances, string filePath = "RustServerManager.json")
        {
            var configs = instances.Select(vm => vm.Instances).ToList();
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void SaveToFile(string filePath = "RustServerManager.json")
        {
            var rawInstances = Instances.Select(i => i.Instance).ToList();
            var json = JsonSerializer.Serialize(rawInstances, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        private void ExecuteConnectToConsole()
        {
            if (SelectedInstance == null)
                return;

            var consoleWindow = new RustServerConsoleWindow(
                SelectedInstance.ServerIp,
                SelectedInstance.RconPort,
                SelectedInstance.RconPassword,
                SelectedInstance.Identity,
                GetLatestLogFilePath(SelectedInstance));

            consoleWindow.Title = $"{SelectedInstance.ServerHostname} Console";
            consoleWindow.Show();
        }

        private string GetLatestLogFilePath(RustInstanceGridItemViewModel instance)
        {
            var logFolder = Path.Combine(
                instance.InstallDirectory,
                "server",
                instance.Identity,
                "rustserverlog");

            Directory.CreateDirectory(logFolder);

            return Path.Combine(logFolder, $"serverlog-{DateTime.Now:MMddyyyy}.log");
        }
    }
}