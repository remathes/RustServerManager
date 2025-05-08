// RustDataCache.cs
using RustServerManager.Models;
using RustServerManager.ViewModels;
using System.Collections.Generic;

namespace RustServerManager.Services
{
    public class RustDataCache
    {
        public static RustDataCache Current { get; private set; } = new RustDataCache();

        public List<RustServerManagerEntry> Servers { get; set; } = new();
        public List<PlayerStatsViewModel> Players { get; set; } = new();
        public Dictionary<string, ItemDefinition> Items { get; set; } = new();

        public static void Load(RustDataCache cache)
        {
            Current = cache;
        }
    }
}