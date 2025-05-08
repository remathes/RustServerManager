namespace RustServerManager.Utils
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    public static class NetworkMonitorOverrides
    {
        private static readonly string FilePath = "NetworkMonitorOverrides.json";

        private class Entry
        {
            public string CounterName { get; set; }
            public string LastKnownIp { get; set; }
        }

        private static Dictionary<string, Entry> _entries;

        static NetworkMonitorOverrides()
        {
            Load();
        }

        public static string GetCounter(string serverName) =>
            _entries.TryGetValue(serverName, out var entry) ? entry.CounterName : null;

        public static (string CounterName, string LastKnownIp) GetConfig(string serverName)
        {
            if (_entries.TryGetValue(serverName, out var entry))
                return (entry.CounterName, entry.LastKnownIp);
            return (null, null);
        }

        public static void Set(string serverName, string counterName, string ip)
        {
            _entries[serverName] = new Entry
            {
                CounterName = counterName,
                LastKnownIp = ip
            };
            Save();
        }

        public static bool HasSetting(string serverName)
        {
            return _entries.ContainsKey(serverName);
        }

        public static void Reset(string serverName)
        {
            if (_entries.Remove(serverName))
                Save();
        }

        private static void Load()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                _entries = JsonSerializer.Deserialize<Dictionary<string, Entry>>(json) ?? new();
            }
            else _entries = new();
        }

        private static void Save()
        {
            var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
