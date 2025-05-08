using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RustServerManager.Services
{
    public class OxideUpdater
    {
        private const string OxideJsonUrl = "https://umod.org/games/rust.json";
        private const string DownloadUrl = "https://github.com/OxideMod/Oxide.Rust/releases/latest/download/Oxide.Rust.zip";
        private string _installDir = $"{Directory.GetCurrentDirectory()}\\Oxide";
        private string DllPath => Path.Combine(_installDir, "RustDedicated_Data", "Managed", "Oxide.Rust.dll");
        private string VersionFile => Path.Combine(_installDir, "version.txt");

        public Func<string, Task>? ReportStatusAsync { get; set; }
        public Func<double, Task>? ReportProgressAsync { get; set; }

        public void SetInstallDirectory(string path)
        {
            _installDir = path;
        }

        public async Task RunUpdateAsync()
        {
            try
            {
                if (IsRustRunning())
                {
                    if (ReportStatusAsync != null) await ReportStatusAsync("RustDedicated is currently running. Skipping update.");
                    return;
                }

                if (ReportStatusAsync != null) await ReportStatusAsync("Checking online version...");
                var onlineVersion = await GetOnlineVersionAsync();

                if (ReportStatusAsync != null) await ReportStatusAsync("Checking local version...");
                var currentVersion = GetCurrentVersion();

                if (currentVersion == null || currentVersion != onlineVersion)
                {
                    if (ReportStatusAsync != null) await ReportStatusAsync("Installing or updating Oxide...");
                    await DownloadAndInstallAsync(onlineVersion);
                }
                else
                {
                    if (ReportStatusAsync != null) await ReportStatusAsync("Oxide is up to date.");
                }
            }
            catch (Exception ex)
            {
                if (ReportStatusAsync != null) await ReportStatusAsync($"An error occurred: {ex.Message}");
            }
        }

        private bool IsRustRunning()
        {
            foreach (var proc in Process.GetProcessesByName("RustDedicated"))
            {
                return true;
            }
            return false;
        }

        private async Task<string> GetOnlineVersionAsync()
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync(OxideJsonUrl);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("latest_release_version").GetString();
        }

        private string? GetCurrentVersion()
        {
            if (!File.Exists(DllPath)) return null;

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(DllPath);
                return versionInfo.FileVersion?.TrimEnd('.', '0');
            }
            catch
            {
                return null;
            }
        }

        private async Task DownloadAndInstallAsync(string version)
        {
            var zipPath = Path.Combine(_installDir, "oxide.zip");

            using var client = new HttpClient();

            if (ReportStatusAsync != null) await ReportStatusAsync("Downloading Oxide...");
            if (ReportProgressAsync != null) await ReportProgressAsync(10);
            var data = await client.GetByteArrayAsync(DownloadUrl);

            await File.WriteAllBytesAsync(zipPath, data);
            if (ReportProgressAsync != null) await ReportProgressAsync(40);

            try
            {
                if (ReportStatusAsync != null) await ReportStatusAsync("Extracting files...");
                ZipFile.ExtractToDirectory(zipPath, _installDir, true);
                File.Delete(zipPath);
                if (ReportProgressAsync != null) await ReportProgressAsync(80);

                await File.WriteAllTextAsync(VersionFile, version);
                if (ReportStatusAsync != null) await ReportStatusAsync($"Installed Oxide version {version}");
                if (ReportProgressAsync != null) await ReportProgressAsync(100);
            }
            catch (Exception ex)
            {
                if (ReportStatusAsync != null) await ReportStatusAsync($"Extraction error: {ex.Message}");
            }
        }
    }
}
