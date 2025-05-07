using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.VisualBasic.Devices;
using RustServerManager.Services;
using RustServerManager.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RustServerManager.ViewModels
{
    public partial class RustServerStatsViewModel : ObservableObject
    {
        private System.Timers.Timer _monitorTimer;
        private Process _rustProcess;
        private RconClient _rconClient;
        private PerformanceCounter _diskReadCounter;
        private PerformanceCounter _diskWriteCounter;
        private PerformanceCounter _diskTransferCounter;
        private PerformanceCounter _bytesSent;
        private PerformanceCounter _bytesReceived;

        [ObservableProperty] private double cpuPercent;
        [ObservableProperty] private double ramUsedGb;
        [ObservableProperty] private double ramTotalGb;
        [ObservableProperty] private double netSent;
        [ObservableProperty] private double netReceived;
        [ObservableProperty] private double driveWrite;
        [ObservableProperty] private double driveRead;
        [ObservableProperty] private double driveTransfer;
        [ObservableProperty] private int playerCount;
        [ObservableProperty] private int sleeperCount;
        [ObservableProperty] private string uptime;
        [ObservableProperty] private string rustChangeset;
        [ObservableProperty] private string localBuildId;
        [ObservableProperty] private bool isOutdated;
        [ObservableProperty] private int fps;
        [ObservableProperty] private int entityCount;
        [ObservableProperty] private string sysinfo;
        [ObservableProperty] private string version;
        [ObservableProperty] private bool isRunning;

        private ObservableCollection<double> _cpuPoints = new();
        private ObservableCollection<double> _ramPoints = new();
        private ObservableCollection<double> _netSentPoints = new();
        private ObservableCollection<double> _netReceivedPoints = new();
        private ObservableCollection<double> _driveWritePoints = new();
        private ObservableCollection<double> _driveReadPoints = new();
        private ObservableCollection<double> _driveTransferPoints = new();

        public ISeries[] CpuSeries { get; private set; }
        public ISeries[] RamSeries { get; private set; }
        public ISeries[] NetSeries { get; private set; }
        public ISeries[] DriveSeries { get; private set; }

        public Axis[] SharedXAxis { get; private set; } = new Axis[] { new Axis { IsVisible = false } };
        public Axis[] CpuYAxis { get; private set; } = new Axis[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = 100 } };
        public Axis[] RamYAxis { get; private set; } = new Axis[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = GetTotalSystemGB()  } };
        public Axis[] NetYAxis { get; private set; } = new Axis[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = 1, AnimationsSpeed = TimeSpan.FromMicroseconds(300) } };
        public Axis[] DriveYAxis { get; private set; } = new Axis[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = 100 } };

        public string RconIp { get; set; }
        public int RconPort { get; set; }
        public string RconPassword { get; set; }

        public string CpuLabel => $"{CpuPercent:0.0}% / 100%";
        public string RamLabel => $"{RamUsedGb:0.0} GB / {RamTotalGb:0.0} GB";
        public string NetLabel => $"{FormatNetValue(netOut)} sent / {FormatNetValue(netIn)} recv";

        public string DriveLabel => $"{DriveRead:0.00} MB/s r | {DriveWrite:0.00} MB/s w | {DriveTransfer:0.00} MB/s t";

        private double GetBytesSent()
        {
            return _bytesSent?.NextValue() ?? 0;
        }

        private double GetBytesReceived()
        {
            return _bytesReceived?.NextValue() ?? 0;
        }

        public RustInstanceGridItemViewModel SelectedInstance { get; set; }
        public string perfName;

        public RustServerStatsViewModel()
        {
            var chartColor = GetChartLineColor();

            _diskReadCounter = new PerformanceCounter("LogicalDisk", "Disk Read Bytes/sec", "_Total");
            _diskWriteCounter = new PerformanceCounter("LogicalDisk", "Disk Write Bytes/sec", "_Total");
            _diskTransferCounter = new PerformanceCounter("LogicalDisk", "Disk Transfers/sec", "_Total");

            _diskReadCounter.NextValue();
            _diskWriteCounter.NextValue();
            _diskTransferCounter.NextValue();

            CpuSeries = new ISeries[]
            {
        new LineSeries<double>
        {
            Values = _cpuPoints,
            GeometrySize = 0,
            Fill = new SolidColorPaint(new SKColor(0, 255, 0, 100)),
            Stroke = new SolidColorPaint(new SKColor(0, 255, 0), 2)
        }
            };

            RamSeries = new ISeries[]
            {
        new LineSeries<double>
        {
            Values = _ramPoints,
            GeometrySize = 0,
            Fill = new SolidColorPaint(new SKColor(0, 255, 0, 100)),
            Stroke = new SolidColorPaint(new SKColor(0, 255, 0), 2)
        }
            };

            NetSeries = new ISeries[]
            {
        new LineSeries<double>
        {
            Values = _netSentPoints,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(80)),
            LineSmoothness = 0.9
        },
        new LineSeries<double>
        {
            Values = _netReceivedPoints,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColors.Green.WithAlpha(80)),
            LineSmoothness = 0.9
        }
            };

            DriveSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _driveReadPoints,
                    GeometrySize = 0,
                    Fill = new SolidColorPaint(new SKColor(255, 255, 0, 100)),
                    Stroke = new SolidColorPaint(SKColors.LimeGreen, 2),
                },
                new LineSeries<double>
                {
                    Values = _driveWritePoints,
                    GeometrySize = 0,
                    Fill = new SolidColorPaint(new SKColor(255, 0, 0, 100)),
                    Stroke = new SolidColorPaint(SKColors.Red, 2),
                },
                new LineSeries<double>
                {
                    Values = _driveTransferPoints,
                    GeometrySize = 0,
                    Fill = new SolidColorPaint(new SKColor(255, 0, 0, 100)),
                    Stroke = new SolidColorPaint(SKColors.Yellow, 2)
                }
            };
        }

        private void AddRollingPoint(ObservableCollection<double> points, double value)
        {
            if (points.Count >= 60)
                points.RemoveAt(0);
            points.Add(value);
        }

        public bool IsConnected => _rconClient.IsConnected;

        public void Start(RustInstanceGridItemViewModel selected)
        {
            if (_bytesReceived != null)
                _bytesReceived.Dispose();
            if (_bytesSent != null)
                _bytesSent.Dispose();
            perfName = NetworkMonitorOverrides.GetCounter(Environment.MachineName);
            if (selected != null)
            {
                if (perfName != null)
                {
                    InitializeCounters(perfName);
                    SelectedInstance = selected;
                    TryAttachToRunningProcess();
                    StartUnifiedMonitor(true);
                }
                else
                {
                    StartUnifiedMonitor(false);
                }
            }
            else
            {
                StartUnifiedMonitor(false);
            }
        }

        public void Stop()
        {
            StartUnifiedMonitor(false);
            StopMonitoring();
        }

        public void Clear()
        {
            CpuPercent = 0;
            RamUsedGb = 0;
            RamTotalGb = 0;
            PlayerCount = 0;
            SleeperCount = 0;
            Fps = 0;
            EntityCount = 0;
            Sysinfo = "0";
            Version = "0";
            IsRunning = false;
            _cpuPoints.Clear();
            _ramPoints.Clear();
            _netReceivedPoints.Clear();
            _netSentPoints.Clear();
            _driveReadPoints.Clear();
            _driveWritePoints.Clear();
            _driveTransferPoints.Clear();
        }

        public void TryAttachToRunningProcess()
        {
            if (_rconClient != null)
            {
                _rconClient = new RconClient(SelectedInstance.Identity);
                RconIp = SelectedInstance.RconIp;
                RconPort = SelectedInstance.RconPort;
                RconPassword = SelectedInstance.RconPassword;
            }
            string exePath = SelectedInstance?.RustDedicatedProcess;
            if (string.IsNullOrWhiteSpace(exePath) || !System.IO.File.Exists(exePath)) return;

            string exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);
            _rustProcess = Process.GetProcessesByName(exeName).FirstOrDefault(p =>
            {
                try { return p.MainModule?.FileName == exePath; } catch { return false; }
            });

            if (_rustProcess != null)
                StartMonitoring();
        }

        private void StartMonitoring()
        {
            _monitorTimer = new System.Timers.Timer(1000);
            _monitorTimer.Elapsed += (_, _) => UpdateMetrics();
            _monitorTimer.Start();
        }

        private void StopMonitoring()
        {
            _monitorTimer?.Stop();
        }

        private async void StartUnifiedMonitor(bool start)
        {
            while (start)
            {
                if (_rustProcess != null && !_rustProcess.HasExited)
                    UpdateMetrics();

                //if (RconIp != null && RconPort.ToString() != null && RconPassword != null)
                //{
                //    if (RconClient.Instance.IsConnected)
                //        await PollRustStatsAsync();
                //    else
                //    {
                //        await RconClient.Instance.EnsureConnectedAsync(RconIp, ushort.Parse(RconPort.ToString()), RconPassword);
                //        if (RconClient.Instance.IsConnected)
                //            await PollRustStatsAsync();
                //    }
                //}

                await Task.Delay(5000);
            }
        }
        private readonly int _networkWindowSize = 60;
        private readonly Queue<double> _netHistory = new();
        void AddRollingPoint(ObservableCollection<ObservableValue> collection, double value)
        {
            collection.Add(new ObservableValue(value));
            if (collection.Count > 60) collection.RemoveAt(0);
        }

        private string FormatNetValue(double kb)
        {
            if (kb < 1)
                return $"{kb * 1024:F0} B/s";
            else if (kb < 1024)
                return $"{kb:F1} KB/s";
            else
                return $"{kb / 1024:F2} MB/s";
        }
        public double netIn = 0;
        public double netOut = 0;
        private void UpdateMetrics() => System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (_rustProcess == null || _rustProcess.HasExited) return;

            try
            {
                _rustProcess.Refresh();
                netIn = _bytesReceived.NextValue() / 1024.0;  // KB/s
                netOut = _bytesSent.NextValue() / 1024.0;     // KB/s
                double netCombined = netIn + netOut;
                _netHistory.Enqueue(netCombined);
                if (_netHistory.Count > _networkWindowSize)
                    _netHistory.Dequeue();


                CpuPercent = Math.Round(GetCpuUsagePercent(_rustProcess), 1);
                RamUsedGb = Math.Round(_rustProcess.WorkingSet64 / (1024.0 * 1024 * 1024), 1);
                RamTotalGb = Math.Round(GetTotalSystemMemoryInGb(), 1);
                NetSent = netOut;
                NetReceived = netIn;
                DriveRead = Math.Round(_diskReadCounter.NextValue() / (1024.0 * 1024), 2);
                DriveWrite = Math.Round(_diskWriteCounter.NextValue() / (1024.0 * 1024), 2);
                DriveTransfer = Math.Round(_diskTransferCounter.NextValue() / (1024.0 * 1024), 2);
                IsRunning = true;
                AddRollingPoint(_cpuPoints, CpuPercent);
                AddRollingPoint(_ramPoints, RamUsedGb);
                AddRollingPoint(_netSentPoints, NetSent);
                AddRollingPoint(_netReceivedPoints, NetReceived);
                AddRollingPoint(_driveReadPoints, DriveRead);
                AddRollingPoint(_driveWritePoints, DriveWrite);
                AddRollingPoint(_driveTransferPoints, DriveTransfer);
                // Notify UI to refresh bound labels
                OnPropertyChanged(nameof(CpuLabel));
                OnPropertyChanged(nameof(RamLabel));
                OnPropertyChanged(nameof(NetLabel));
                OnPropertyChanged(nameof(DriveLabel));
            }
            catch
            {
                IsRunning = false;
            }
        });

        public void InitializeCounters(string perfName)
        {
            _bytesSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", perfName);
            _bytesReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", perfName);

            // Prime them
            _bytesSent.NextValue();
            _bytesReceived.NextValue();
        }

        private double GetCpuUsagePercent(Process process)
        {
            using var cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
            cpuCounter.NextValue();
            return cpuCounter.NextValue() / Environment.ProcessorCount;
        }

        private static double GetTotalSystemGB()
        {
            var info = new ComputerInfo();
            return info.TotalPhysicalMemory / (1024.0 * 1024 * 1024);
        }

        private double GetTotalSystemMemoryInGb()
        {
            var info = new ComputerInfo();
            return info.TotalPhysicalMemory / (1024.0 * 1024 * 1024);
        }

        private static SKColor GetChartLineColor()
        {
            if (App.Current?.Resources["ChartLineColor"] is SolidColorBrush brush)
            {
                var c = brush.Color;
                return new SKColor(c.R, c.G, c.B, c.A);
            }
            return SKColors.LimeGreen;
        }

        public async Task PollRustStatsAsync()
        {
            try
            {
                RustServerMonitorService rsms = new RustServerMonitorService(this, SelectedInstance.InstallDirectory, SelectedInstance.RconIp, SelectedInstance.RconPort, SelectedInstance.RconPassword);

                var status = await _rconClient.SendCommandAsync("status");
                var perf = await _rconClient.SendCommandAsync("perf");
                var sysinfo = await _rconClient.SendCommandAsync("global.sysinfo");
                var version = await _rconClient.SendCommandAsync("global.version");

                foreach (var line in status.Split('\n'))
                {
                    if (line.StartsWith("players", StringComparison.OrdinalIgnoreCase))
                        PlayerCount = int.Parse(line.Split(':')[1].Split('(')[0].Trim());

                    if (line.StartsWith("sleepers", StringComparison.OrdinalIgnoreCase))
                        SleeperCount = int.Parse(line.Split(':')[1].Trim());

                    if (line.StartsWith("entities", StringComparison.OrdinalIgnoreCase))
                        EntityCount = int.Parse(line.Split(':')[1].Trim());
                }

                var fpsMatch = Regex.Match(perf, @"fps:\s*(\d+)");
                if (fpsMatch.Success)
                    Fps = int.Parse(fpsMatch.Groups[1].Value);

                if (sysinfo.StartsWith("Up ", StringComparison.OrdinalIgnoreCase))
                    Sysinfo = sysinfo[3..].Trim();
                if (version.StartsWith("Ve ", StringComparison.OrdinalIgnoreCase))
                    Version = version;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("[RCON Monitor] Failed: " + ex.Message);
            }
        }
    }
}