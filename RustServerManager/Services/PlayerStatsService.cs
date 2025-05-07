using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RustServerManager.ViewModels;


namespace RustServerManager.Services
{
    public static class PlayerStatsService
    {
        public static List<PlayerStatsViewModel> GetMockPlayers()
        {
            return new List<PlayerStatsViewModel>
        {
            new PlayerStatsViewModel {
                Name = "RustMaster99",
                SteamId = "76561198000000001",
                Kills = 27,
                Deaths = 13,
                Headshots = 11,
                ResourcesGathered = 30422,
                ItemsLooted = 452,
                StructuresBuilt = 58,
                FavoriteWeapon = "rifle.ak",
                FavoriteResource = "wood",
                Playtime = TimeSpan.FromHours(12.3),
                LastSeen = DateTime.UtcNow.AddHours(-2)
            },
            new PlayerStatsViewModel {
                Name = "SniperWolf",
                SteamId = "76561198000000002",
                Kills = 56,
                Deaths = 9,
                Headshots = 42,
                ResourcesGathered = 10822,
                ItemsLooted = 981,
                StructuresBuilt = 76,
                FavoriteWeapon = "rifle.lr300",
                FavoriteResource = "stone",
                Playtime = TimeSpan.FromHours(24.7),
                LastSeen = DateTime.UtcNow.AddHours(-6)
            }
        };
        }
    }

}
