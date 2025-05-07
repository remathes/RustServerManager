using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Geo;
using Ookii.Dialogs.Wpf;
using RustServerManager.Utils;
using RustServerManager.ViewModels;
using System.Diagnostics;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Security.Permissions;

namespace RustServerManager.Views
{
    /// <summary>
    /// Interaction logic for ServerConfigView.xaml
    /// </summary>
    public partial class ServerConfigView : UserControl
    {
        public ServerConfigViewModel ViewModel { get; set; }
        public ServerConfigView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
            Loaded += ServerConfigView_Loaded;
        }

        private void ServerConfigView_Loaded(object sender, RoutedEventArgs e)
        {

        }

    //    [RelayCommand]
    //    public void LoadServerCfg()
    //    {
    //        var dialog = new VistaOpenFileDialog
    //        {
    //            Title = "Select server.cfg",
    //            Filter = "Rust Config (*.cfg)|*.cfg",
    //            CheckFileExists = true,
    //            Multiselect = false,
    //            InitialDirectory = Directory.GetCurrentDirectory()
    //        };

    //        if (dialog.ShowDialog() == true)
    //        {
    //            if (LoadFromServerCfg(dialog.FileName))
    //            {
    //                MessageBox.Show("Loaded server config");
    //                if (ViewModel != null)
    //                {
    //                    if (ViewModel.WorldSize != "")
    //                    {
    //                       CBWorldSize.SelectedIndex = CBWorldSize.Items.IndexOf(int.Parse(ViewModel.WorldSize));
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public bool LoadFromServerCfg(string path)
    //    {
    //        if (!File.Exists(path))
    //            return false;

    //        var lines = File.ReadAllLines(path);
    //        foreach (var line in lines)
    //        {
    //            var trimmed = line.Trim();
    //            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//")) continue;

    //            var parts = trimmed.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
    //            if (parts.Length < 2) continue;

    //            var key = parts[0].Trim();
    //            var value = parts[1].Trim().Trim('"');

    //            switch (key.ToLowerInvariant())
    //            {
    //                case "server.ip":
    //                    ViewModel.ServerIp = value;
    //                    break;
    //                case "server.port":
    //                    ViewModel.ServerIp = value;
    //                    break;
    //                case "server.identity":
    //                    ViewModel.Identity = value;
    //                    break;
    //                case "server.hostname":
    //                    ViewModel.ServerHostname = value;
    //                    break;
    //                case "server.description":
    //                    ViewModel.Description = value;
    //                    break;
    //                case "server.worldsize":
    //                    ViewModel.WorldSize = value;
    //                    break;
    //                case "server.seed":
    //                    ViewModel.Seed = value;
    //                    break;
    //                case "rcon.web":
    //                    ViewModel.RconWeb = value;
    //                    break;
    //                case "server.maxplayers":
    //                    ViewModel.MaxPlayers = value;
    //                    break;
    //                case "server.tickrate":
    //                    ViewModel.ServerTickrate = value;
    //                    break;
    //                case "server.saveinterval":
    //                    ViewModel.ServerSaveInterval = value;
    //                    break;
    //                case "rcon.ip":
    //                    ViewModel.RconIp = value;
    //                    break;
    //                case "rcon.port":
    //                    ViewModel.RconPort = value;
    //                    break;
    //                case "rcon.password":
    //                    ViewModel.RconPassword = value;
    //                    break;
    //            }
    //        }
    //        if (ValidateConfig())
    //            return true;
    //        else
    //            return false;
    //    }

    //    [RelayCommand]
    //    private void BrowseSteamCmd()
    //    {
    //        var dialog = new VistaFolderBrowserDialog
    //        {
    //            Description = "Select or create a folder where SteamCMD will be installed to",
    //            ShowNewFolderButton = true,
    //            UseDescriptionForTitle = true
    //        };
    //        if (dialog.ShowDialog() == true)
    //        {
    //            ViewModel.SteamCmdPath = dialog.SelectedPath;
    //            ViewModel.SteamCmdPath = Path.Combine(dialog.SelectedPath, "steamcmd.exe");
    //        }
    //    }

    //    [RelayCommand]
    //    private async Task DownloadSteamAsync()
    //    {
    //        if (string.IsNullOrEmpty(ViewModel.SteamCmdPath))
    //        {
    //            MessageBox.Show("Select or create a folder where SteamCMD will be installed to");
    //            return;
    //        }
    //        string url = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

    //        string zipPath = System.IO.Path.Combine(ViewModel.SteamCmdPath, "steamcmd.zip");

    //        if (!Directory.Exists(ViewModel.SteamCmdPath))
    //            Directory.CreateDirectory(ViewModel.SteamCmdPath);

    //        try
    //        {
    //            using var client = new HttpClient();
    //            var data = await client.GetByteArrayAsync(url);
    //            await File.WriteAllBytesAsync(zipPath, data);

    //            ZipFile.ExtractToDirectory(zipPath, ViewModel.SteamCmdPath, true);
    //            MessageBox.Show("? SteamCMD downloaded and extracted to:\n" + ViewModel.SteamCmdPath);
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show("? Error downloading SteamCMD:\n" + ex.Message);
    //        }
    //    }
    //    [RelayCommand]
    //    private void RustServerUpdated()
    //    {
    //        if (string.IsNullOrEmpty(ViewModel.SteamCmdPath))
    //        {
    //            MessageBox.Show("Select or create a folder where SteamCMD will be installed to");
    //            return;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.InstallDirectory))
    //        {
    //            MessageBox.Show("Select or create a folder where Rust will be installed to");
    //            return;
    //        }
    //        ProcessStartInfo psi = new ProcessStartInfo();
    //        psi.FileName = "RustUpdate.exe";
    //        psi.Arguments = $"\"{ViewModel.SteamCmdPath}\\{ViewModel.SteamCmdExe}\" \"{ViewModel.InstallDirectory}\"";
    //        psi.UseShellExecute = false;
    //        using (Process process = new Process())
    //        {
    //            process.StartInfo = psi;
    //            process.Start();
    //            process.WaitForExit();
    //        }
    //    }

    //    bool ValidateConfig()
    //    {
    //        if (string.IsNullOrEmpty(ViewModel.InstallDirectory))
    //        {
    //            MessageBox.Show("Select or create a folder where Rust will be installed to");
    //            return false;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.ServerHostname))
    //        {
    //            MessageBox.Show("Please enter a value for Host Name");
    //            return false;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.Description))
    //        {
    //            MessageBox.Show("Please enter a value Server Description");
    //            return false;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.ServerIp))
    //        {
    //            MessageBox.Show("Please enter a valid value for Server IP Address");
    //            return false;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.ServerPort))
    //        {
    //            MessageBox.Show("Please enter a valid Server Port");
    //            return false;
    //        }
    //        if (ViewModel.UseCustomMap)
    //        {
    //            if (string.IsNullOrEmpty(ViewModel.ServerLevelUrl))
    //            {
    //                MessageBox.Show("Please enter a value for Server Level Url.\nExanple: https://mymap/mymap.map");
    //                return false;
    //            }
    //        }
    //        else
    //        {
    //            if (string.IsNullOrEmpty(ViewModel.Seed))
    //            {
    //                MessageBox.Show("Please generate a seed to use");
    //                return false;
    //            }
    //            if (string.IsNullOrEmpty(ViewModel.WorldSize))
    //            {
    //                MessageBox.Show("Please enter a value between 1000 and 6000 for World Size");
    //                return false;
    //            }
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.RconIp))
    //        {
    //            MessageBox.Show("Please enter a value for RCON Ip Address");
    //            return false;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.RconPort))
    //        {
    //            MessageBox.Show("Please enter a value for RCON Port");
    //            return false;
    //        }
    //        if (string.IsNullOrEmpty(ViewModel.RconPassword))
    //        {
    //            MessageBox.Show("Please generate a RCON Password");
    //            return false;
    //        }
    //        return true;
    //    }

    //    [RelayCommand]
    //    private void BrowseInstallDirectory()
    //    {
    //        var dialog = new VistaFolderBrowserDialog();
    //        if (dialog.ShowDialog() == true)
    //        {
    //            ViewModel.InstallDirectory = dialog.SelectedPath;
    //        }
    //    }

    //    [RelayCommand]
    //    private void GenerateRconPassword()
    //    {
    //        ViewModel.RconPassword = Password.Generate(6, 0);
    //    }

    //    [RelayCommand]
    //    private void GenerateSeed()
    //    {
    //        var random = new Random();
    //        ViewModel.Seed = random.Next(1, 999999).ToString(); // 6-digit seed
    //    }

    //    [RelayCommand]
    //    private void GenerateIdentity()
    //    {
    //        ViewModel.Identity = System.Guid.NewGuid().ToString();
    //    }

    //    [RelayCommand]
    //    private void SaveConfig()
    //    {
    //        SaveServerCfg();
    //    }
    //    void SaveServerCfg()
    //    {
    //        if (string.IsNullOrWhiteSpace(ViewModel.InstallDirectory) || string.IsNullOrWhiteSpace(ViewModel.Identity))
    //        {
    //            MessageBox.Show("Install path or identity not set.");
    //            return;
    //        }

    //        var sb = new StringBuilder();
    //        sb.AppendLine($"server.ip {ViewModel.ServerIp}");
    //        sb.AppendLine($"server.port {ViewModel.ServerPort}");
    //        sb.AppendLine($"rcon.ip {ViewModel.RconIp}");
    //        sb.AppendLine($"rcon.port {ViewModel.RconPort}");
    //        sb.AppendLine($"rcon.password \"{ViewModel.RconPassword}\"");
    //        sb.AppendLine($"rcon.web {ViewModel.RconWeb}");
    //        sb.AppendLine($"server.identity \"{ViewModel.Identity}\"");
    //        sb.AppendLine($"server.hostname \"{ViewModel.ServerHostname}\"");
    //        sb.AppendLine($"server.description \"{ViewModel.Description}\"");
    //        sb.AppendLine($"server.maxplayers {ViewModel.MaxPlayers}");
    //        sb.AppendLine($"server.tickrate {ViewModel.ServerTickrate}");
    //        sb.AppendLine($"sever.saveinterval {ViewModel.ServerSaveInterval}");
    //        if (!ViewModel.UseCustomMap)
    //        {
    //            sb.AppendLine($"server.seed {ViewModel.Seed}");
    //            sb.AppendLine($"server.worldsize {ViewModel.WorldSize}");
    //        }
    //        else
    //        {
    //            sb.AppendLine($"server.levelurl \"{ViewModel.ServerLevelUrl}\"");
    //        }
    //        if (ValidateConfig())
    //        {
    //            ServerBuildManager.WriteServerCfg(ViewModel.InstallDirectory, ViewModel.Identity, sb.ToString());
    //            SaveSettings();
    //            bool valid_save = LoadFromServerCfg(System.IO.Path.Combine(ViewModel.InstallDirectory, "server", ViewModel.Identity, "cfg", "server.cfg"));
    //            if (valid_save)
    //            {
    //                MessageBox.Show("Server Config Saved");
    //            }
    //            else
    //            {
    //                MessageBox.Show("Server Config was saved but is not valid please re-check all values");
    //            }
    //        }
    //    }

    //    [RelayCommand]
    //    private void StartServer()
    //    {
    //        Process process = Process.GetProcessesByName("RustDedicated").FirstOrDefault();
    //        if (process != null && !process.HasExited)
    //        {
    //            Debug.WriteLine("Server is online connecting...");
    //            return;
    //        }

    //        string exePath = Path.Combine(ViewModel.InstallDirectory, "RustDedicated.exe");
    //        if (!File.Exists(exePath))
    //        {
    //            MessageBox.Show($"RustDedicated.exe not found in {ViewModel.InstallDirectory} aborting start up");
    //            return;
    //        }
    //        if (string.IsNullOrEmpty($"{ViewModel.InstallDirectory}\\server\\{ViewModel.Identity}"))
    //            MessageBox.Show($"Server Identity not found in {ViewModel.InstallDirectory}\\server path. Please make sure one exists");
    //            return;

    //        if (!Directory.Exists($"{ViewModel.InstallDirectory}\\server\\{ViewModel.Identity}\\logs"))
    //            Directory.CreateDirectory($"{ViewModel.InstallDirectory}\\server\\{ViewModel.Identity}\\logs");
    //        string args = "";
    //        if (ViewModel.UseCustomMap)
    //        {
    //            if (string.IsNullOrEmpty(ViewModel.ServerIp) 
    //                || string.IsNullOrEmpty(ViewModel.ServerPort) 
    //                || string.IsNullOrEmpty(ViewModel.RconIp) 
    //                || string.IsNullOrEmpty(ViewModel.RconPassword) 
    //                || string.IsNullOrEmpty(ViewModel.ServerLevelUrl)
    //                || string.IsNullOrEmpty(ViewModel.ServerHostname) 
    //                || string.IsNullOrEmpty(ViewModel.Description))
    //                return;
    //            args = $"-batchmode " +
    //                $"+server.identity \"{ViewModel.Identity}\" " +
    //                $"+server.ip {ViewModel.ServerIp} " +
    //                $"+server.port {ViewModel.ServerPort} " +
    //                $"+server.tickrate {ViewModel.ServerTickrate} " +
    //                $"+server.hostname \"{ViewModel.ServerHostname}\" " +
    //                $"+server.description \"{ViewModel.Description}\" " +
    //                $"+server.saveinterval {ViewModel.ServerSaveInterval} " +
    //                $"+server.maxplayers {ViewModel.MaxPlayers} " +
    //                $"+rcon.ip {ViewModel.RconIp} " +
    //                $"+rcon.password \"{ViewModel.RconPassword}\" " +
    //                $"+rcon.web {ViewModel.RconWeb} " + 
    //                $"+server.levelurl \"{ViewModel.ServerLevelUrl}\" " +
    //                $"-logfile \"{ViewModel.InstallDirectory}\\server\\{ViewModel.Identity}\\rustserverlog\\server-{DateTime.Now.ToString("MMddyyyy")}.log\"";
    //        }
    //        else
    //        {
    //            if (string.IsNullOrEmpty(ViewModel.ServerIp) 
    //                || string.IsNullOrEmpty(ViewModel.ServerPort) 
    //                || string.IsNullOrEmpty(ViewModel.RconIp) 
    //                || string.IsNullOrEmpty(ViewModel.RconPassword) 
    //                || string.IsNullOrEmpty(ViewModel.Seed)
    //                || string.IsNullOrEmpty(ViewModel.WorldSize) 
    //                || string.IsNullOrEmpty(ViewModel.ServerHostname) 
    //                || string.IsNullOrEmpty(ViewModel.Description))
    //                return;
    //            args = $"-batchmode " +
    //                $"+server.identity \"{ViewModel.Identity}\" " +
    //                $"+server.ip {ViewModel.ServerIp} " +
    //                $"+server.port {ViewModel.ServerPort} " +
    //                $"+server.tickrate {ViewModel.ServerTickrate} " +
    //                $"+server.hostname \"{ViewModel.ServerHostname}\" " +
    //                $"+server.description \"{ViewModel.Description}\" " +
    //                $"+server.saveinterval {ViewModel.ServerSaveInterval} " +
    //                $"+server.maxplayers {ViewModel.MaxPlayers} " +
    //                $"+rcon.ip {ViewModel.RconIp} +rcon.password \"{ViewModel.RconPassword}\" " +
    //                $"+rcon.web {ViewModel.RconWeb} " + 
    //                $"+server.seed {ViewModel.Seed} " +
    //                $"+server.worldsize {ViewModel.WorldSize} " +
    //                $"-logfile \"{ViewModel.InstallDirectory}\\server\\{ViewModel.Identity}\\rustserverlog\\server-{DateTime.Now.ToString("MMddyyyy")}.log\"";
    //        }
    //        var startInfo = new ProcessStartInfo
    //        {
    //            FileName = exePath,
    //            Arguments = args,
    //            WorkingDirectory = ViewModel.InstallDirectory,
    //            //RedirectStandardOutput = true,
    //            //RedirectStandardError = true,
    //            UseShellExecute = false,
    //            CreateNoWindow = true
    //        };

    //        Process serverProcess = new Process { StartInfo = startInfo };
    //        //serverProcess.OutputDataReceived += (s, e) => {
    //        //    if (e.Data != null)
    //        //        AppendHighlightedOutput(e.Data);
    //        //};
    //        //serverProcess.ErrorDataReceived += (s, e) => {
    //        //    if (e.Data != null)
    //        //        RconConsoleControl.AppendHighlightedOutput(e.Data);
    //        //};

    //        serverProcess.Start();
    //        //serverProcess.BeginOutputReadLine();
    //        //serverProcess.BeginErrorReadLine();
    //        //ServerOutput += "\nServer started.";
    //    }

    //    [RelayCommand]
    //    private void StopServer()
    //    {
    //        Process process = Process.GetProcessesByName("RustDedicated").FirstOrDefault();
    //        if (process != null && !process.HasExited)
    //        {
    //            process.Kill();
    //            process = null;
    //            //ServerOutput += "\nServer stopped.";
    //            //ConsoleHeader = "Console: Server Offline";
    //        }
    //        else
    //        {
    //            //ServerOutput += "\nNo server is currently running.";
    //        }
    //    }

    //    private void SaveSettings()
    //    {
    //        if (!Directory.Exists(Path.Combine(ViewModel.InstallDirectory, "server", ViewModel.Identity, "rustserverlog")))
    //            Directory.CreateDirectory(Path.Combine(ViewModel.InstallDirectory, "server", ViewModel.Identity, "rustserverlog"));
    //        JsonSerializerOptions opt = new JsonSerializerOptions();
    //        opt.WriteIndented = true;
    //        var settings = JsonSerializer.Serialize(ViewModel, opt);
    //        File.WriteAllText("RustServerManager.json", settings);
    //    }
    }
}
