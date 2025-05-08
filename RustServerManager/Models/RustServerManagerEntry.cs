using RustServerManager.ViewModels;
using System.Collections.Generic;

namespace RustServerManager.Models
{
    public class RustServerManagerEntry
    {
        public int Id { get; set; }
        public string Identity { get; set; }
        public string ServerHostname { get; set; }
        public string InstallDirectory { get; set; }
        public DatabaseConfig DatabaseConfig { get; set; }
        public List<PlayerStatsViewModel> Players { get; set; } = new();
    }

    public class DatabaseConfig
    {
        public string MySqlHost { get; set; }
        public int MySqlPort { get; set; } = 3306;
        public string MySqlUsername { get; set; }
        public string MySqlPassword { get; set; }
        public string MySqlDatabaseName { get; set; }
    }

}
