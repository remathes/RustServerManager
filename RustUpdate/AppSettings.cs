using System;
using System.IO;
using System.Text.Json;

namespace RustUpdate
{
    public class AppSettings
    {
        public string? SteamCmdExePath {  get; set; }
        public string? SteamCMDPath { get; set; }
        public string? RustInstallPath { get; set; }
        public DateTime? LastUpdateCheck { get; set; }
        public bool EnableAutoUpdateCheck { get; set; }

        public bool ShouldCheckForUpdates()
        {
            return LastUpdateCheck == null || LastUpdateCheck.Value.Date < DateTime.Today;
        }

        public void MarkUpdateChecked()
        {
            LastUpdateCheck = DateTime.Now;
            Save();
        }

        public static AppSettings Load(string path = "AppSettings.json")
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json)!;
        }

        public void Save(string path = "AppSettings.json")
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
