using System;

namespace RustServerManager.ViewModels
{
    // PlayerStatsViewModel.cs
    public class PlayerStatsViewModel
    {
        public string Name { get; set; }
        public string SteamId { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public double KDR => Deaths == 0 ? Kills : Math.Round((double)Kills / Deaths, 2);
        public int Headshots { get; set; }
        public int ResourcesGathered { get; set; }
        public int ItemsLooted { get; set; }
        public int StructuresBuilt { get; set; }
        public string FavoriteWeapon { get; set; }
        public string FavoriteResource { get; set; }
        public TimeSpan Playtime { get; set; }
        public DateTime LastSeen { get; set; }

        public string FavoriteItemIconPath => $"Assets/ItemIcons/{FavoriteWeapon}.png";
    }
}
