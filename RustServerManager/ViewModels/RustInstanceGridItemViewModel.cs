using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using RustServerManager.Models;
using RustServerManager.Utils;
using RustServerManager.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

namespace RustServerManager.ViewModels
{
    public partial class RustInstanceGridItemViewModel : ObservableObject
    {
        // Gauge Series
        public ObservableCollection<PieSeries<ObservableValue>> CpuGaugeSeries { get; set; } = new();
        public ObservableCollection<PieSeries<ObservableValue>> RamGaugeSeries { get; set; } = new();
        public CancellationTokenSource cts { get;set;}
        private Task metricsTask;
        CancellationToken token;
        // CPU %
        private double _cpuPercent;
        public double CpuPercent
        {
            get => _cpuPercent;
            set
            {
                if (_cpuPercent != value)
                {
                    _cpuPercent = value;
                    OnPropertyChanged();
                    UpdateGauge(CpuGaugeSeries, CpuPercent);
                    OnPropertyChanged(nameof(CpuPercentText));
                }
            }
        }
        public string CpuPercentText => $"{CpuPercent:F1}% / 100%";

        // RAM %
        private double _ramUsedGb;
        public double RamUsedGb
        {
            get => _ramUsedGb;
            set
            {
                if (_ramUsedGb != value)
                {
                    _ramUsedGb = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RamDisplayText));
                    OnPropertyChanged(nameof(RamUsedDisplayText));
                }
            }
        }

        private double _totalSystemRamGb;
        public double TotalSystemRamGb
        {
            get => _totalSystemRamGb;
            set
            {
                if (_totalSystemRamGb != value)
                {
                    _totalSystemRamGb = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RamDisplayText));
                    OnPropertyChanged(nameof(RamTotalDisplayText));
                }
            }
        }

        // Optional if you still want % also
        private double _ramPercent;
        public double RamPercent
        {
            get => _ramPercent;
            set
            {
                if (_ramPercent != value)
                {
                    _ramPercent = value;
                    OnPropertyChanged();
                    UpdateGauge(RamGaugeSeries, RamPercent); // <-- still update gauge
                    OnPropertyChanged(nameof(RamDisplayText)); // <-- update text too
                }
            }
        }

        // THIS is the label shown on the gauge
        public string RamDisplayText => $"{RamUsedGb:F1} GB / {TotalSystemRamGb:F1} GB";
        public string RamUsedDisplayText => $"{RamUsedGb:F1} GB";
        public string RamTotalDisplayText => $"{TotalSystemRamGb:F1} GB";

        // Gauge Updater
        private void UpdateGauge(ObservableCollection<PieSeries<ObservableValue>> gauge, double percent)
        {
            string[] segmentNames = { "Low", "Medium", "High", "Max","Na" };
            percent = Math.Clamp(percent, 0.01, 100.0);
            double remaining = 100 - percent;

            // Calculate values for each 25% segment
            double[] zoneValues = new double[4];
            if (percent > 0) zoneValues[0] = Math.Min(percent, 25);
            if (percent > 25) zoneValues[1] = Math.Min(percent - 25, 25);
            if (percent > 50) zoneValues[2] = Math.Min(percent - 50, 25);
            if (percent > 75) zoneValues[3] = Math.Min(percent - 75, 25);

            // Segment colors
            SKColor[] colors =
            {
                new SKColor(0, 200, 0),      // Green
                new SKColor(255, 215, 0),    // Yellow
                new SKColor(255, 140, 0),    // Orange
                new SKColor(220, 20, 60)     // Red
            };

            int expectedSegments = 4 + 1;

            // Initialize once
            if (gauge.Count != expectedSegments)
            {
                gauge.Clear();

                for (int i = 0; i < 4; i++)
                {
                    gauge.Add(new PieSeries<ObservableValue>
                    {
                        Name = segmentNames[i],
                        Values = new ObservableCollection<ObservableValue> { new ObservableValue(zoneValues[i]) },
                        InnerRadius = 60,
                        MaxRadialColumnWidth = 50,
                        Stroke = null,
                        Fill = new SolidColorPaint(colors[i]),
                        IsHoverable = false,
                        DataLabelsPaint = null
                    });
                }

                // Remaining (gray)
                gauge.Add(new PieSeries<ObservableValue>
                {
                    Name = segmentNames[4],
                    Values = new ObservableCollection<ObservableValue> { new ObservableValue(remaining) },
                    InnerRadius = 60,
                    MaxRadialColumnWidth = 50,
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(230, 230, 230)),
                    IsHoverable = false,
                    DataLabelsPaint = null
                });
            }
            else
            {
                // Just update values — no flicker
                for (int i = 0; i < 4; i++)
                {
                    if (gauge[i].Values.FirstOrDefault() is ObservableValue v)
                        v.Value = zoneValues[i];
                }

                if (gauge[4].Values.FirstOrDefault() is ObservableValue r)
                    r.Value = remaining;
            }
        }

        public static double GetTotalPhysicalMemoryGb()
        {
            var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            return computerInfo.TotalPhysicalMemory / 1024.0 / 1024.0 / 1024.0;
        }

        public void StartMetricsUpdate(RustServerInstance instance)
        {

            try
            {
                cts = new CancellationTokenSource();
                token = cts.Token;

                metricsTask = Task.Run(async () =>
                {
                    try
                    {
                        var exeName = Path.GetFileNameWithoutExtension(instance.RustDedicatedProcess);
                        var process = Process.GetProcessesByName(exeName)
                            .FirstOrDefault(p => string.Equals(p.MainModule?.FileName, instance.RustDedicatedProcess, StringComparison.OrdinalIgnoreCase));

                        if (process == null)
                        {
                            Application.Current.Dispatcher.Invoke(ResetGauges);
                            return;
                        }

                        using var cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);

                        double totalRam = GetTotalPhysicalMemoryGb();
                        TotalSystemRamGb = totalRam;

                        while (!token.IsCancellationRequested)
                        {
                            try
                            {
                                if (process.HasExited)
                                    break;

                                process.Refresh();

                                var cpuUsage = cpuCounter.NextValue() / Environment.ProcessorCount;
                                var ramUsed = process.WorkingSet64 / 1024.0 / 1024.0 / 1024.0;
                                var ramPercent = (ramUsed / totalRam) * 100.0;

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CpuPercent = cpuUsage;
                                    RamUsedGb = ramUsed;
                                    RamPercent = ramPercent;

                                    UpdateGauge(RamGaugeSeries, RamPercent);
                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Metrics loop error: {ex.Message}");
                                break;
                            }

                            await Task.Delay(1000, token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected — do nothing
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Metrics start error: {ex.Message}");
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(ResetGauges);
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start metrics update: {ex}");
            }
        }

        public void StopMetricsUpdate()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
            }

            metricsTask = null;
        }

        private void ResetGauges()
        {
            UpdateGauge(CpuGaugeSeries, 0);
            UpdateGauge(RamGaugeSeries, 0);
        }

        private readonly Timer _monitorTimer;
        public RustServerInstance Instance { get; }
        private readonly ObservableCollection<UserControl> _pages;
        private int _currentPageIndex;

        [ObservableProperty]
        private UserControl currentPage;


        public IRelayCommand BackCommand { get; }
        public IRelayCommand NextCommand { get; }

        public RustInstanceGridItemViewModel(RustServerInstance instance)
        {
            Instance = instance;
            // ✅ Initialize values from model
            _autoStart = instance.AutoStart;
            _autoUpdate = instance.AutoUpdate;
            _monitorTimer = new Timer(1000);
            _monitorTimer.Elapsed += (_, _) => UpdateStatus();
            _monitorTimer.Start();
            _pages = new ObservableCollection<UserControl>(LoadPages());
            _currentPageIndex = 0;
            StartMetricsUpdate(Instance);
            BackCommand = new RelayCommand(GoBack, CanGoBack);
            NextCommand = new RelayCommand(GoNext, CanGoNext);
            UpdateCurrentPage();
        }

        private void GoBack()
        {
            if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                UpdateCurrentPage();
            }
        }

        private void GoNext()
        {
            if (_currentPageIndex < _pages.Count - 1)
            {
                _currentPageIndex++;
                UpdateCurrentPage();
            }
        }

        private bool CanGoBack() => _currentPageIndex > 0;
        private bool CanGoNext() => _currentPageIndex < _pages.Count - 1;

        private void UpdateCurrentPage()
        {
            CurrentPage = _pages[_currentPageIndex];
            BackCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }
        //Refresh UI
        public void RefreshFromInstance()
        {
            //OnPropertyChanged(nameof(BannerImagePath));
            OnPropertyChanged(nameof(Identity));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(ServerHostname));
            OnPropertyChanged(nameof(ServerIp));
            OnPropertyChanged(nameof(ServerPort));
            OnPropertyChanged(nameof(RconIp));
            OnPropertyChanged(nameof(RconPort));
            OnPropertyChanged(nameof(RconPassword));
            OnPropertyChanged(nameof(AppPort));
            OnPropertyChanged(nameof(RconWeb));
            OnPropertyChanged(nameof(UseCustomMap));
            OnPropertyChanged(nameof(MapName));
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(ServerLevelUrl));
            OnPropertyChanged(nameof(WorldSize));
            OnPropertyChanged(nameof(Seed));
            OnPropertyChanged(nameof(MaxPlayers));
            OnPropertyChanged(nameof(RustDedicatedProcess));
            OnPropertyChanged(nameof(InstallDirectory));
            //OnPropertyChanged(nameof(SteamCMDPath));
            OnPropertyChanged(nameof(ServerCfg));
            OnPropertyChanged(nameof(ServerTickrate));
            OnPropertyChanged(nameof(ServerSaveInterval));
            OnPropertyChanged(nameof(MySqlHost));
            OnPropertyChanged(nameof(MySqlPort));
            OnPropertyChanged(nameof(MySqlUserName));
            OnPropertyChanged(nameof(MySqlPassword));
            OnPropertyChanged(nameof(MySqlDatabaseName));
            OnPropertyChanged(nameof(LastWiped));
            //OnPropertyChanged(nameof(ProcessId));
        }
        private IEnumerable<UserControl> LoadPages()
        {
            yield return new RustServerGridView { DataContext = this };
        }

        public string BannerImagePath => Instance.BannerImagePath;
        public string Identity => Instance.Identity;
        public string Description => Instance.Description;
        public string ServerHostname => Instance.ServerHostname;
        public string ServerIp => Instance.ServerIp;
        public int ServerPort => Instance.ServerPort;
        public string RconIp => Instance.RconIp;
        public int RconPort => Instance.RconPort;
        public string RconPassword => Instance.RconPassword;
        public int AppPort => Instance.AppPort;
        public int RconWeb => Instance.RconWeb;
        public bool UseCustomMap => Instance.UseCustomMap;
        public string MapName => Instance.MapName;
        public string Level => Instance.Level;
        public string ServerLevelUrl => Instance.ServerLevelUrl;
        public int WorldSize => Instance.WorldSize;
        public int Seed => Instance.Seed;
        public int MaxPlayers => Instance.MaxPlayers;
        public string RustDedicatedProcess => Instance.RustDedicatedProcess;
        public string InstallDirectory => Instance.InstallDirectory;
        public string SteamCMDPath => Instance.SteamCmdPath;
        public string ServerCfg => Instance.ServerCfg;
        public int ServerTickrate => Instance.ServerTickrate;
        public int ServerSaveInterval => Instance.ServerSaveInterval;
        public string MySqlHost => Instance.MySqlHost;
        public int MySqlPort => Instance.MySqlPort;
        public string MySqlUserName => Instance.MySqlUsername;
        public string MySqlPassword => Instance.MySqlPassword;
        public string MySqlDatabaseName => Instance.MySqlDatabaseName;
        public bool EnableGracefulShutdown => Instance.EnableGracefulShutdown;
        public int ShutdownDelaySeconds => Instance.ShutdownDelaySeconds;
        public string ShutdownMessageCommand => Instance.ShutdownMessageCommand;


        private bool _autoStart;
        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                if (_autoStart != value)
                {
                    _autoStart = value;
                    Instance.AutoStart = value;
                    OnPropertyChanged();
                    UpdateToggleInDatabase("AutoStart", value);
                }
            }
        }

        private bool _autoUpdate;
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                if (_autoUpdate != value)
                {
                    _autoUpdate = value;
                    Instance.AutoUpdate = value;
                    OnPropertyChanged();
                    UpdateToggleInDatabase("AutoUpdate", value);
                }
            }
        }

        private int _processId;
        public int ProcessId
        {
            get => _processId;
            set
            {
                if(_processId !=value)
                {
                    _processId = value;
                    Instance.ProcessId = value;
                    OnPropertyChanged();
                    UpdateProcessIdInDatabase("ProcessId", value);
                }
            }
        }

        private string _uptime = "--:--";
        public string Uptime
        {
            get => _uptime;
            set
            {
                if (_uptime != value)
                {
                    _uptime = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime LastWiped => Instance.LastWiped;
        public DatabaseConfig DatabaseConfig => Instance.DatabaseConfig;

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private void TrySetMapName()
        {
            try
            {
                string mapFolder = Path.Combine(InstallDirectory, "server", Identity);
                if (Directory.Exists(mapFolder))
                {
                    string? mapFilePath = Directory.GetFiles(mapFolder, "*.map", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (!string.IsNullOrEmpty(mapFilePath))
                    {
                        string newMapName = Path.GetFileName(mapFilePath);
                        if (!string.IsNullOrEmpty(newMapName))
                        {
                            Instance.MapName = newMapName;

                            // Optional: also update in MySQL instantly!
                            UpdateMapNameInDatabase(newMapName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle error (just in case)
                Console.WriteLine($"Failed to detect map file: {ex.Message}");
            }
        }

        private void UpdateMapNameInDatabase(string mapName)
        {
            string connectionString = DatabaseHelper.GetConnectionString();
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "UPDATE Instances SET MapName = @MapName WHERE Identity = @Identity", conn))
                {
                    cmd.Parameters.AddWithValue("@MapName", mapName);
                    cmd.Parameters.AddWithValue("@Identity", Identity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateToggleInDatabase(string column, bool value)
        {
            try
            {
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(DatabaseHelper.GetConnectionString());
                conn.Open();

                string query = $"UPDATE Instances SET {column} = @value WHERE Identity = @identity";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@identity", Instance.Identity);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to update {column}: {ex.Message}");
            }
        }

        private void UpdateProcessIdInDatabase(string column, int value)
        {
            try
            {
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(DatabaseHelper.GetConnectionString());
                conn.Open();

                string query = $"UPDATE Instances SET {column} = @value WHERE Identity = @identity";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@identity", Instance.Identity);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to update {column}: {ex.Message}");
            }
        }

        public void UpdateStatus()
        {
            try
            {
                var exeName = Path.GetFileNameWithoutExtension(Instance.RustDedicatedProcess);
                var matchingProcess = Process.GetProcessesByName(exeName)
                    .FirstOrDefault(p => string.Equals(p.MainModule?.FileName, Instance.RustDedicatedProcess, StringComparison.OrdinalIgnoreCase));

                if (matchingProcess != null)
                {
                    IsRunning = true;

                    // Calculate uptime
                    if (IsRunning && (string.IsNullOrEmpty(MapName) || MapName == "No map file yet"))
                    {
                        TrySetMapName();
                    }
                    var uptimeSpan = DateTime.Now - matchingProcess.StartTime;
                    this.Uptime = $"{(int)uptimeSpan.TotalHours:D2} hr(s) {uptimeSpan.Minutes:D2} min(s)";
                }
                else
                {
                    IsRunning = false;
                    this.Uptime = "-- hr -- min"; // fallback when not running
                }
            }
            catch
            {
                IsRunning = false;
                this.Uptime = "-- hr -- min"; // fallback in case of exception too
            }
        }
    }
}
