using RustServerManager.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RustServerManager.Utils
{
    public static class ServerBuildManager
    {
        public static async Task<bool> InstallRustServerAsync(string steamCmdPath, string installDir, Action<string> progressCallback)
        {
            if (!File.Exists(steamCmdPath))
            {
                progressCallback("SteamCMD not found.");
                return false;
            }

            progressCallback("Starting Rust server installation...");
            var args = $"+login anonymous +force_install_dir \"{installDir}\" +app_update 258550 validate +quit";

            var startInfo = new ProcessStartInfo
            {
                FileName = steamCmdPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(steamCmdPath) ?? ""
            };

            var process = new Process { StartInfo = startInfo };

            process.OutputDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) progressCallback(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) progressCallback("ERR: " + e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            progressCallback("Installation completed.");
            return process.ExitCode == 0;
        }

        public static void WriteServerCfg(string installDir, string identity, string configText)
        {
            var cfgPath = Path.Combine(installDir, "server", identity, "cfg");
            if(!Directory.Exists(cfgPath))
                Directory.CreateDirectory(cfgPath);
            File.WriteAllText(Path.Combine(cfgPath, "server.cfg"), configText);
        }
    }
}
