using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RustServerManager.Utils
{
    public static class MySqlInstaller
    {
        private static readonly string InstallerUrl = "https://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-8.3.0.0.msi";

        public static async Task<bool> DownloadAndLaunchMySqlInstallerAsync(string downloadPath)
        {
            if (IsMySqlInstalled())
            {
                Debug.WriteLine("MySQL is already installed.");
                return true;
            }

            string installerPath = Path.Combine(downloadPath, "mysql-installer-8.3.0.0.msi");

            try
            {
               Directory.CreateDirectory(downloadPath);

                // Download only if not already present
                if (!File.Exists(installerPath))
                {
                    using var httpClient = new HttpClient();
                    using var response = await httpClient.GetAsync(InstallerUrl);
                    response.EnsureSuccessStatusCode();

                    await using (var stream = await response.Content.ReadAsStreamAsync())
                    await using (var fileStream = File.Create(installerPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                // Launch MSI (GUI installer)
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        UseShellExecute = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                // After user completes wizard, check again
                return IsMySqlInstalled();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MySQL install failed: {ex.Message}");
                return false;
            }
        }

        private static bool IsMySqlInstalled()
        {
            try
            {
                return ServiceController.GetServices().Any(s =>
                    s.ServiceName.Contains("mysql", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }
    }
}
