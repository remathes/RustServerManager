using Newtonsoft.Json;

namespace RustServerManager.Models
{
    public class ServerStatsModel
    {
        [JsonProperty("Hostname")]
        public string Hostname { get; set; }

        [JsonProperty("MaxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonProperty("Players")]
        public int Players { get; set; }

        [JsonProperty("Queued")]
        public int Queued { get; set; }

        [JsonProperty("Joining")]
        public int Joining { get; set; }

        [JsonProperty("ReservedSlots")]
        public int ReservedSlots { get; set; }

        [JsonProperty("EntityCount")]
        public int EntityCount { get; set; }

        [JsonProperty("GameTime")]
        public string GameTime { get; set; }

        [JsonProperty("Uptime")]
        public int Uptime { get; set; }

        [JsonProperty("Map")]
        public string Map { get; set; }

        [JsonProperty("Framerate")]
        public float Framerate { get; set; }

        [JsonProperty("Memory")]
        public int Memory { get; set; }

        [JsonProperty("MemoryUsageSystem")]
        public int MemoryUsageSystem { get; set; }

        [JsonProperty("Collections")]
        public int Collections { get; set; }

        [JsonProperty("NetworkIn")]
        public long NetworkIn { get; set; }

        [JsonProperty("NetworkOut")]
        public long NetworkOut { get; set; }

        [JsonProperty("Restarting")]
        public bool Restarting { get; set; }

        [JsonProperty("SaveCreatedTime")]
        public string SaveCreatedTime { get; set; }

        [JsonProperty("Version")]
        public int Version { get; set; }

        [JsonProperty("Protocol")]
        public string Protocol { get; set; }
    }
}
