using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;

namespace RustServerManager.Services
{
    public class RustStatsCollector
    {
        private readonly Timer _pollTimer;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        private readonly string _processName = "RustDedicated";

        private Process? _rustProcess;
        private string? _rustDrive;

        public event Action<StatsSnapshot>? StatsUpdated;
        public void Tick() => Poll();
        public RustStatsCollector(string? driveOverride = null)
        {
            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", "Idle");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            _rustDrive = driveOverride;

            _pollTimer = new Timer(1000);
            _pollTimer.Elapsed += (s, e) => Poll();
            _pollTimer.Start();
        }

        private void Poll()
        {
            _rustProcess ??= Process.GetProcessesByName(_processName).FirstOrDefault();
            if (_rustProcess == null || _rustProcess.HasExited)
            {
                StatsUpdated?.Invoke(new StatsSnapshot { IsRunning = false });
                return;
            }

            if (_rustDrive == null)
            {
                var path = _rustProcess.MainModule?.FileName;
                if (path != null)
                    _rustDrive = Path.GetPathRoot(path);
            }

            var cpu = GetCpuUsage(_rustProcess);
            var mem = _rustProcess.WorkingSet64 / (1024.0 * 1024.0);
            var (readMB, writeMB) = GetDiskIO(_rustDrive);
            var (netIn, netOut) = GetNetworkIO();

            StatsUpdated?.Invoke(new StatsSnapshot
            {
                IsRunning = true,
                CpuUsage = cpu,
                MemoryMB = mem,
                DiskReadMB = readMB,
                DiskWriteMB = writeMB,
                NetInKB = netIn,
                NetOutKB = netOut
            });
        }

        private double GetCpuUsage(Process proc)
        {
            using var cpu = new PerformanceCounter("Process", "% Processor Time", proc.ProcessName);
            cpu.NextValue();
            System.Threading.Thread.Sleep(200); // short delay to get real reading
            return cpu.NextValue() / Environment.ProcessorCount;
        }

        private (double readMB, double writeMB) GetDiskIO(string? drive)
        {
            if (string.IsNullOrEmpty(drive)) return (0, 0);

            // Get the drive letter without the trailing backslash, e.g., "C:"
            string instance = drive?.Substring(0, 1) + ":";

            try
            {
                var diskReads = new PerformanceCounter("LogicalDisk", "Disk Read Bytes/sec", instance);
                var diskWrites = new PerformanceCounter("LogicalDisk", "Disk Write Bytes/sec", instance);

                // Prime the counters
                diskReads.NextValue();
                diskWrites.NextValue();
                System.Threading.Thread.Sleep(200);

                var readMB = diskReads.NextValue() / (1024.0 * 1024.0);
                var writeMB = diskWrites.NextValue() / (1024.0 * 1024.0);

                return (readMB, writeMB);
            }
            catch
            {
                // If the drive doesn't exist or can't be monitored
                return (0, 0);
            }
        }
        private long _lastTotalIn = 0;
        private long _lastTotalOut = 0;
        private DateTime _lastNetSampleTime = DateTime.Now;
        private (double inKB, double outKB) GetNetworkIO()
        {
            double totalIn = 0, totalOut = 0;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var stats = ni.GetIPv4Statistics();
                totalIn += stats.BytesReceived;
                totalOut += stats.BytesSent;
            }

            var now = DateTime.Now;
            var duration = (now - _lastNetSampleTime).TotalSeconds;
            _lastNetSampleTime = now;

            double inRate = (totalIn - _lastTotalIn) / duration / 1024.0;
            double outRate = (totalOut - _lastTotalOut) / duration / 1024.0;

            _lastTotalIn = (long)totalIn;
            _lastTotalOut = (long)totalOut;

            return (inRate, outRate);
        }
    }

    public class StatsSnapshot
    {
        public bool IsRunning { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryMB { get; set; }
        public double DiskReadMB { get; set; }
        public double DiskWriteMB { get; set; }
        public double NetInKB { get; set; }
        public double NetOutKB { get; set; }
    }
}