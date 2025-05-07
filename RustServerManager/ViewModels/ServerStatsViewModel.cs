using Newtonsoft.Json;
using RustServerManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RustServerManager.ViewModels;
using RustServerManager.Utils;
using RustServerManager.Services;
using System.Diagnostics;

namespace RustServerManager.ViewModels
{
    public class ServerStatsViewModel
    {
        private readonly RconClient _rconClient;
        RustInstanceGridItemViewModel ViewModel { get; set; }
        public ServerStatsViewModel()
        {
            if(_rconClient == null)
                _rconClient = new RconClient(ViewModel.Identity);
        }

        async Task GetServerStats()
        {
            try
            {
                string rconJson = await _rconClient.SendCommandAsync("serverbridge.stats");

                int jsonStart = rconJson.IndexOf('{');
                if (jsonStart >= 0)
                {
                    string cleanJson = rconJson.Substring(jsonStart).Trim();
                    var stats = JsonConvert.DeserializeObject<ServerStatsModel>(cleanJson);

                    // TODO: Apply to UI
                    Debug.WriteLine($"Players: {stats.Players}, FPS: {stats.Framerate}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get server stats: {ex.Message}");
            }
        }
    }
}
