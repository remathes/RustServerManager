using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustServerManager.ViewModels
{
    public class PlayerStatsTabViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PlayerStatsViewModel> Players { get; set; } = new();
        public PlayerStatsViewModel SelectedPlayer { get; set; }

        // Later bind this to your selected item
        //public void LoadPlayers()
        //{
        //    Players.Clear();

        //    using (var conn = new MySqlConnection("your-connection-string"))
        //    {
        //        conn.Open();
        //        string query = "SELECT p.steam_id, p.name, ps.kills, ps.deaths, ps.headshots, ps.gathered, " +
        //                       "ps.looted_items, ps.structures_built, ps.favorite_weapon, ps.favorite_resource, " +
        //                       "p.last_seen, p.total_played " +
        //                       "FROM players p " +
        //                       "JOIN player_stats ps ON p.steam_id = ps.steam_id";

        //        using (var cmd = new MySqlCommand(query, conn))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                Players.Add(new PlayerStatsViewModel
        //                {
        //                    SteamId = reader["steam_id"].ToString(),
        //                    Name = reader["name"].ToString(),
        //                    Kills = Convert.ToInt32(reader["kills"]),
        //                    Deaths = Convert.ToInt32(reader["deaths"]),
        //                    Headshots = Convert.ToInt32(reader["headshots"]),
        //                    ResourcesGathered = Convert.ToInt32(reader["gathered"]),
        //                    ItemsLooted = Convert.ToInt32(reader["looted_items"]),
        //                    StructuresBuilt = Convert.ToInt32(reader["structures_built"]),
        //                    FavoriteWeapon = reader["favorite_weapon"].ToString(),
        //                    FavoriteResource = reader["favorite_resource"].ToString(),
        //                    LastSeen = Convert.ToDateTime(reader["last_seen"]),
        //                    Playtime = TimeSpan.TryParse(reader["total_played"]?.ToString(), out var pt) ? pt : TimeSpan.Zero
        //                });
        //            }
        //        }
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
