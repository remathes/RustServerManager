using System;
using System.IO;
using System.Text.Json;

namespace RustServerManager.Utils
{
    public static class DatabaseHelper
    {
        private static string? _connectionString;

        public static string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString; // Already cached

            try
            {
                string configPath = "dbconfig.json";

                if (!File.Exists(configPath))
                    throw new FileNotFoundException("dbconfig.json not found.");

                var json = File.ReadAllText(configPath);

                var config = JsonSerializer.Deserialize<DatabaseConfig>(json);

                if (config == null)
                    throw new Exception("Failed to deserialize dbconfig.json.");

                _connectionString = $"Server={config.MySqlHost};Port={config.MySqlPort};Database={config.MySqlDatabaseName};Uid={config.MySqlUsername};Pwd={config.MySqlPassword};";
                return _connectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading database config: {ex.Message}");
                throw;
            }
        }

        private class DatabaseConfig
        {
            public string MySqlHost { get; set; } = "";
            public int MySqlPort { get; set; } = 3306;
            public string MySqlDatabaseName { get; set; } = "";
            public string MySqlUsername { get; set; } = "";
            public string MySqlPassword { get; set; } = "";
        }
    }
}