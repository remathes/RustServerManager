
// RustServerMonitorService.cs
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using RustServerManager.ViewModels;
using RustServerManager.Services;

namespace RustServerManager.Services
{
    public class RustServerMonitorService
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);
        private CancellationTokenSource _cts;
        private readonly RustServerStatsViewModel _vm;
        private readonly RconClient _rconClient;
        private readonly string _installDirectory;
        private readonly string _rconIp;
        private readonly int _rconPort;
        private readonly string _rconPassword;

        public RustServerMonitorService(RustServerStatsViewModel viewModel, string installDirectory, string rconIp, int rconPort, string rconPassword)
        {
            _vm = viewModel;
            _installDirectory = installDirectory;
            if (_rconClient == null)
            {
                _rconClient = new RconClient(viewModel.SelectedInstance.Identity);
                _rconIp = rconIp;
                _rconPort = rconPort;
                _rconPassword = rconPassword;
            }
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => MonitorLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }

        private async Task MonitorLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {

                    await _rconClient.EnsureConnectedAsync(_rconIp, (ushort)_rconPort, _rconPassword);
                    if (_rconClient.IsConnected)
                        await UpdateStatsAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[MonitorService] Error: " + ex.Message);
                }

                await Task.Delay(_interval, token);
            }
        }

        private async Task UpdateStatsAsync()
        {
            //string sysinfo = await RconClient.Instance.SendCommandAsync("global.sysinfo");
            //string version = await RconClient.Instance.SendCommandAsync("global.version");

            //// RAM from system block
            //var ram = Regex.Match(sysinfo, @"Memory:\s+(\d+)\s+MB", RegexOptions.IgnoreCase);
            //if (ram.Success) _vm.RamTotalGb = Math.Round(int.Parse(ram.Groups[1].Value) / 1024.0, 1);

            //// Process memory
            ////var procMem = Regex.Match(sysinfo, @"Process[\s\S]+?Memory:\s+(\d+)\s+MB", RegexOptions.IgnoreCase);
            ////if (procMem.Success) _vm.RamUsedGb = Math.Round(int.Parse(procMem.Groups[1].Value) / 1024.0, 1);

            //// Entities + Sleepers
            //var entsMatch = Regex.Match(sysinfo, @"([\d,]+)\s+ents,\s+(\d+)\s+slprs", RegexOptions.IgnoreCase);
            //if (entsMatch.Success)
            //{
            //    _vm.EntityCount = int.Parse(entsMatch.Groups[1].Value.Replace(",", ""));
            //    _vm.SleeperCount = int.Parse(entsMatch.Groups[2].Value);
            //}

            //// FPS
            //var fpsMatch = Regex.Match(sysinfo, @"(\d+)fps");
            //if (fpsMatch.Success) _vm.Fps = int.Parse(fpsMatch.Groups[1].Value);

            //// Uptime
            //var uptimeMatch = Regex.Match(sysinfo, @"(\d+d\d+h\d+m\d+s)");
            //if (uptimeMatch.Success) _vm.Uptime = uptimeMatch.Groups[1].Value;

            //// Players [0/2]
            //var playerMatch = Regex.Match(sysinfo, @"\[(\d+)/\d+\]");
            //if (playerMatch.Success) _vm.PlayerCount = int.Parse(playerMatch.Groups[1].Value);

            //// Version from global.version
            //var buildMatch = Regex.Match(version, @"Changeset:\s+(\d+)");
            //if (buildMatch.Success) _vm.RustChangeset = buildMatch.Groups[1].Value;

            //var manifestPath = Path.Combine(_installDirectory, "steamapps", "appmanifest_258550.acf");
            //if (File.Exists(manifestPath))
            //{
            //    var file = File.ReadAllText(manifestPath);
            //    var localMatch = Regex.Match(file, "\"buildid\"\\s+\"(\\d+)\"");
            //    if (localMatch.Success) _vm.LocalBuildId = localMatch.Groups[1].Value;

            //    _vm.IsOutdated = _vm.RustChangeset != _vm.LocalBuildId;
            //}
        }
    }

}