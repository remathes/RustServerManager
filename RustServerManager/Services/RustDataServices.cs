using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using RustServerManager.Models;
using RustServerManager.ViewModels;
using RustServerManager.Services;
using System.Data;

namespace RustServerManager.Services
{

    public class RustDataService
    {
        private readonly string _connectionString;

        public RustDataService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<RustDataCache> LoadAllAsync()
        {
            var cache = new RustDataCache
            {
                Servers = await LoadServersAsync(),
                Players = await LoadPlayersAsync(),
                Items = await LoadItemsAsync()
            };
            return cache;
        }

        private async Task<List<RustServerManagerEntry>> LoadServersAsync()
        {
            var list = new List<RustServerManagerEntry>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("SELECT * FROM servers", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new RustServerManagerEntry
                {
                    Identity = reader.GetString("identity"),
                    ServerHostname = reader.GetString("name")
                });
            }
            return list;
        }

        private async Task<List<PlayerStatsViewModel>> LoadPlayersAsync()
        {
            var list = new List<PlayerStatsViewModel>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("SELECT * FROM players p LEFT JOIN player_stats s ON p.steam_id = s.steam_id", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new PlayerStatsViewModel
                {
                    SteamId = reader.GetInt64("steam_id").ToString(),
                    Name = reader.GetString("name"),
                    Kills = reader.GetInt32("kills"),
                    Deaths = reader.GetInt32("deaths"),
                    Headshots = reader.GetInt32("headshots"),
                    ResourcesGathered = reader.GetInt32("gathered"),
                    ItemsLooted = reader.GetInt32("looted_items"),
                    StructuresBuilt = reader.GetInt32("structures_built"),
                    FavoriteWeapon = reader.GetString("favorite_weapon"),
                    FavoriteResource = reader.GetString("favorite_resource")
                });
            }
            return list;
        }

        private async Task<Dictionary<string, ItemDefinition>> LoadItemsAsync()
        {
            var dict = new Dictionary<string, ItemDefinition>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("SELECT * FROM item_definitions", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = new ItemDefinition
                {
                    Shortname = reader.GetString("shortname"),
                    DisplayName = reader.GetString("name"),
                    MaxStack = reader.GetInt32("stackable")
                };
                dict[item.Shortname] = item;
            }
            return dict;
        }
    }
}
