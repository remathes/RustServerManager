using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows; // for UI updates
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox; // for TextBlock

namespace RustUpdate
{
    public class RustUpdater
    {
        public string SteamCMDPath { get; }
        public string RustInstallPath { get; }
        public string SteamCmdExePath { get; }
        public string OxidePath => Path.Combine(RustInstallPath, "RustDedicated_Data", "Managed");
        public string OxideRustDllPath => Path.Combine(RustInstallPath, "RustDedicated_Data", "Managed", "Oxide.Rust.dll");
        public string SteamManifestPath => Path.Combine(RustInstallPath, "steamapps", "appmanifest_258550.acf");
        public string RustExePath => Path.Combine(RustInstallPath, "RustDedicated.exe");
        public string RustManifest => Path.Combine(RustInstallPath, "steamapps", "appmanifest_258550.acf");
        private string VersionFile => Path.Combine(RustInstallPath, "version.txt");

        public RustUpdater(string steamCMDPath, string rustInstallPath, string steamCMDExePath)
        {
            SteamCMDPath = steamCMDPath;
            RustInstallPath = rustInstallPath;
            SteamCmdExePath = steamCMDExePath;
        }

        public async Task<(string installed, string latest)> GetRustVersionsAsync()
        {
            string installed = "Not Found";
            if (!string.IsNullOrEmpty(SteamCmdExePath))
            {
                installed = "Not Found";
            }

            if (File.Exists(RustManifest))
            {
                string jsonrust = string.Empty;
                try
                {
                    SteamAppManifest manifest = new SteamAppManifest(RustManifest);
                    if (manifest.Data.ContainsKey("buildid"))
                    {
                        installed = manifest.Data["buildid"];
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }

            string? latest = null;
            using var http = new HttpClient();
            try
            {
                var json = await http.GetStringAsync("https://api.steamcmd.net/v1/info/258550");
                using var doc = JsonDocument.Parse(json);
                latest = doc.RootElement.GetProperty("data").GetProperty("258550").GetProperty("depots").GetProperty("branches").GetProperty("public").GetProperty("buildid").GetString();
            }
            catch (Exception ex) { MessageBox.Show($"Error checking for lasted rust server version: {ex.Message}"); return ("Not Found",""); }

            return (installed, latest);
        }

        public string GetInstalledOxideVersion()
        {
            if (!Directory.Exists(Path.Combine(RustInstallPath, "RustDedicated_Data")))
            {
                return "Not Found";
            }

            return GetFileVersion(OxideRustDllPath);
        }

        public async Task<string> GetLatestOxideVersionAsync()
        {
            using var http = new HttpClient();
            try
            {
                var response = await http.GetStringAsync("https://umod.org/games/rust.json");
                if (response != null)
                {
                    using var doc = JsonDocument.Parse(response);
                    return doc.RootElement.GetProperty("latest_release_version").GetString() + ".0";
                }
            }
            catch { }
            return null;
        }

        private readonly string DownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

        public async Task<bool>InstallSteamCMDAsync(TextBlock statusLabel, MainWindow mw,string targetDirectory)
        {
            string zipPath = Path.Combine(targetDirectory, "steamcmd.zip");
            string exePath = Path.Combine(targetDirectory, "steamcmd.exe");
            string extractPath = targetDirectory;

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            // Step 1: Download steamcmd.zip
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(DownloadUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Step 2: Extract
            ZipFile.ExtractToDirectory(zipPath, targetDirectory, overwriteFiles: true);
            File.Delete(zipPath);

            // Step 3: Run steamcmd.exe to let it install itself
            if (File.Exists(exePath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = targetDirectory,
                        Arguments = "+quit", // Just launches then quits after self-update
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void UpdateRustServer(TextBlock statusLabel, MainWindow mw)
        {
            bool isInstalling = !Directory.Exists(RustInstallPath) || GetFileVersion(RustExePath) == "Not Found";
            string action = isInstalling ? "Installing Rust Server..." : "Updating Rust Server...";

            System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = action);

            var args = $"+force_install_dir \"{RustInstallPath}\" +login anonymous +app_update 258550 validate +quit";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(SteamCMDPath,"steamcmd.exe"),
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            process.WaitForExit();

            string result = isInstalling ? "Rust Server installation complete." : "Rust Server update complete.";
            System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = result);
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                mw.LoadVersions();
            });
        }

        private async Task DownloadAndInstallAsync(TextBlock statusLabel)
        {
            var zipPath = Path.Combine(Path.GetTempPath(), "oxide.zip");
            string DownloadUrl = "https://github.com/OxideMod/Oxide.Rust/releases/latest/download/Oxide.Rust.zip";
            using var client = new HttpClient();
            System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = "Downloding Oxide...");
            var data = await client.GetByteArrayAsync(DownloadUrl);

            await File.WriteAllBytesAsync(zipPath, data);

            try
            {
                if (File.Exists(OxideRustDllPath))
                    File.Delete(OxideRustDllPath);
                System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = "Extracting files...");
                ZipFile.ExtractToDirectory(zipPath, RustInstallPath, true);
                File.Delete(zipPath);
                string version = GetInstalledOxideVersion();
                await File.WriteAllTextAsync(VersionFile, version);
                System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = "Install Completed.");
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = "Error installing oxide: "+ex.Message);
            }
        }

        public async void UpdateOxide(TextBlock statusLabel,MainWindow mw)
        {
            string current_oxide = "0.0.0000.0";
            if(File.Exists(VersionFile))
               current_oxide = File.ReadAllText(VersionFile);
            var processes = Process.GetProcessesByName("RustDedicated.exe");
            if (processes.Length > 0)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = "Install aborted RustDedicated.exe is running.");
                return;
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() => statusLabel.Text = "Oxide getting things ready...");
            await DownloadAndInstallAsync(statusLabel);
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                mw.LoadVersions();
            });
        }

        public static string GetFileVersion(string path)
        {
            return File.Exists(path) ? FileVersionInfo.GetVersionInfo(path).FileVersion : "Not Found";
        }
    }
}