using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using RustServerManager.Models;
using RustServerManager.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RustServerManager.ViewModels
{
    public class RustInstanceEditViewModel : INotifyPropertyChanged
    {
        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _installDirectory;
        public string InstallDirectory
        {
            get => _installDirectory;
            set
            {
                if (_installDirectory != value)
                {
                    _installDirectory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanInstallRust));
                }
            }
        }
        private string _steamCmdPath;
        public string SteamCmdPath
        {
            get => _steamCmdPath;
            set
            {
                if (_steamCmdPath != value)
                {
                    _steamCmdPath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanInstallRust));
                }
            }
        }

        public bool CanInstallRust =>
        !string.IsNullOrWhiteSpace(SteamCmdPath) && File.Exists(Path.Combine(SteamCmdPath, "steamcmd.exe")) &&
        !string.IsNullOrWhiteSpace(InstallDirectory) && Directory.Exists(InstallDirectory);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Identity { get; set; } = GenerateSafeIdentity();
        public DateTime LastWiped { get; set; } = DateTime.MinValue;

        public string Description { get; set; }
        public string ServerHostname { get; set; }
        private string _serverUrl;
        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                if (_serverUrl != value)
                {
                    _serverUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MySqlDatabaseName { get; set; } = "rustserverinstances";
        public string MySqlHost { get; set; } = "localhost";
        public string MySqlUserName { get; set; } = "root";

        private string _mySqlPassword;
        public string MySqlPassword
        {
            get => _mySqlPassword;
            set
            {
                if (_mySqlPassword != value)
                {
                    _mySqlPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MySqlPort { get; set; } = 3306;
        public string ServerIp { get; set; } = "0.0.0.0";
        private int _serverPort;
        public int ServerPort
        {
            get => _serverPort;
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RconIp { get; set; } = "0.0.0.0";
        private int _rconPort;
        public int RconPort
        {
            get => _rconPort;
            set
            {
                if(_rconPort != value)
                {  
                    _rconPort = value; 
                    OnPropertyChanged();
                }
            }
        }
        private int _appPort;
        public int AppPort
        {
            get => _appPort;
            set
            {
                if(_appPort != value)
                {
                    _appPort = value;
                    OnPropertyChanged();
                }    
            }
        }
        public string RconPassword { get; set; } = GenerateSafeRconPassword();


        public int RconWeb { get; set; } = 0;
        private bool _useCustomMap;
        public bool UseCustomMap
        {
            get => _useCustomMap;
            set
            {
                if (_useCustomMap != value)
                {
                    _useCustomMap = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _autoUpdate;
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                if (_autoUpdate != value)
                {
                    _autoUpdate = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _autoStart;
        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                if (_autoStart != value)
                {
                    _autoStart = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MapName { get; set; }
        public int MaxPlayers { get; set; } = 100;
        public string ServerLevelUrl { get; set; }

        private int _worldSize;
        public int WorldSize
        {
            get => _worldSize;
            set
            {
                if (_worldSize != value)
                {
                    _worldSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Level { get; set; } = "Procedural Map";

        //public int Seed { get; set; }
        private int _seed;
        public int Seed
        {
            get => _seed;
            set
            {
                if (_seed != value)
                {
                    _seed = value;
                    OnPropertyChanged();
                }
            }
        }
        public string _rustDedicatedProcess;
        public string RustDedicatedProcess
        {
            get => _rustDedicatedProcess;
            set
            {
                if (_rustDedicatedProcess != value)
                {
                    _rustDedicatedProcess = value;
                }
                this.ValidateInstall(out string errorMessage);
                OnPropertyChanged();
            }
        }

        public int ServerTickrate { get; set; } = 1;
        public int ServerSaveInterval { get; set; } = 300;

        private string _serverCfg;
        public string ServerCfg
        {
            get => _serverCfg;
            set
            {
                if (_serverCfg != value)
                {
                    _serverCfg = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableGracefulShutdown;
        public bool EnableGracefulShutdown
        {
            get => _enableGracefulShutdown;
            set
            {
                if (_enableGracefulShutdown != value)
                {
                    _enableGracefulShutdown = value;
                    OnPropertyChanged(nameof(EnableGracefulShutdown));
                }
            }
        }
        private int _shutdownDelaySeconds = 30;
        public int ShutdownDelaySeconds
        {
            get => _shutdownDelaySeconds;
            set
            {
                if (_shutdownDelaySeconds != value)
                {
                    _shutdownDelaySeconds = value;
                    OnPropertyChanged(nameof(ShutdownDelaySeconds));
                }
            }
        }
        private string _shutdownMessageCommand = "say Server shutting down in {seconds} seconds";
        public string ShutdownMessageCommand
        {
            get => _shutdownMessageCommand;
            set
            {
                if (_shutdownMessageCommand != value)
                {
                    _shutdownMessageCommand = value;
                    OnPropertyChanged(nameof(ShutdownMessageCommand));
                }
            }
        }

        public ICommand RunInstallCommand => new AsyncRelayCommand(async () =>
        {
            string mapFolder = Path.Combine(InstallDirectory, "server", Identity);
            string? mapFilePath = Directory.Exists(mapFolder)
                ? Directory.GetFiles(mapFolder, "*.map", SearchOption.TopDirectoryOnly).FirstOrDefault()
                : null;

            string MapFile = string.Empty;

            if (!string.IsNullOrEmpty(mapFilePath))
            {
                MapFile = Path.GetFileName(mapFilePath);
            }
            else
            {
                MapFile = "No map file yet";
            }
            if (!ValidateInstall(out string error))
            {
                MessageBox.Show(error, "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editDialogSession = DialogHost.GetDialogSession("MainDialog");
            editDialogSession?.Close(true);
            bool success = await InstallRustAsync();
            MessageBox.Show(
                success ? "✅ Rust installed successfully." : "❌ Install failed. Check log for details.",
                success ? "Install Complete" : "Install Failed",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Error
            );
        });

        public async Task SetPortsAsync()
        {
            const int baseServerPort = 28015;
            const int baseRconPort = 28016;
            const int baseAppPort = 28017;

            string query = "SELECT ServerPort, RconPort, AppPort FROM Instances";
            string connectionString = DatabaseHelper.GetConnectionString();

            var usedServerPorts = new HashSet<int>();
            var usedRconPorts = new HashSet<int>();
            var usedAppPorts = new HashSet<int>();

            using var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usedServerPorts.Add(reader.GetInt32(0));
                usedRconPorts.Add(reader.GetInt32(1));
                usedAppPorts.Add(reader.GetInt32(2));
            }

            ServerPort = FindNextAvailablePort(baseServerPort, usedServerPorts);
            RconPort = FindNextAvailablePort(baseRconPort, usedRconPorts);
            AppPort = FindNextAvailablePort(baseAppPort, usedAppPorts);
        }

        private int FindNextAvailablePort(int startingPort, HashSet<int> usedPorts)
        {
            int port = startingPort;
            while (usedPorts.Contains(port))
            {
                port++;
            }
            return port;
        }

        public IRelayCommand GenerateSeedCommand { get; }
        public IRelayCommand GenerateIdentityCommand { get; }
        public IRelayCommand GenerateRconPasswordCommand { get; }

        public RustInstanceEditViewModel()
        {
            GenerateSeedCommand = new RelayCommand(GenerateSeed);
            GenerateIdentityCommand = new RelayCommand(GenerateIdentity);
            GenerateRconPasswordCommand = new RelayCommand(GenerateRconPassword);
        }

        private void GenerateSeed()
        {
            if (WorldSize == 0)
                WorldSize = 3000;
            var rng = new Random();
            Seed = rng.Next(1, int.MaxValue);
        }

        public void GenerateRconPassword()
        {
            GenerateSafeRconPassword();
        }

        private const string SafeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
        public static string GenerateSafeRconPassword(int length = 12)
        {
            if (length < 8) throw new ArgumentException("RCON password should be at least 8 characters.");

            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[length];
            rng.GetBytes(buffer);

            return new string(buffer.Select(b => SafeChars[b % SafeChars.Length]).ToArray());
        }

        private void GenerateIdentity()
        {
            var identity = GenerateSafeIdentity();
            Identity = identity;
        }

        private static string GenerateSafeIdentity()
        {
            var baseName = "rust_srv_" + Guid.NewGuid().ToString("N")[..8];
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(baseName.Where(ch => !invalidChars.Contains(ch)).ToArray());
        }

        public async Task<bool> ConfigureMySqlAsync(RustServerInstance instance)
        {
            var session = await DialogHost.Show(new DatabaseConfigDialog(instance), "MainDialog") as DialogSession;
            session?.Close();
            return true;
        }

        public async Task<bool> InstallRustAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Identity))
                {
                    GenerateIdentity();
                }
                if (string.IsNullOrEmpty(ServerCfg))
                {
                    ServerCfg = "";
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "RustUpdate.exe",
                    Arguments = $"\"{SteamCmdPath}\" \"{InstallDirectory}\"",
                    CreateNoWindow = false,
                    UseShellExecute = true
                };

                using (var process = Process.Start(psi))
                {
                    await process.WaitForExitAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Install failed: {ex.Message}");
                return false;
            }
        }

        public bool ValidateInstall(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(SteamCmdPath))
            {
                errorMessage = "⚠ Please set the SteamCMD path.";
                return false;
            }

            if (!File.Exists(Path.Combine(SteamCmdPath, "steamcmd.exe")))
            {
                errorMessage = "⚠ SteamCMD path is not valid.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(InstallDirectory))
            {
                errorMessage = "⚠ Please set the install directory.";
                return false;
            }

            if (!Directory.Exists(InstallDirectory))
            {
                errorMessage = "⚠ Install directory does not exist.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Identity))
            {
                errorMessage = "⚠ Server identity is required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public RustInstanceEditViewModel(RustServerInstance source)
        {
            Identity = source.Identity;
            Description = source.Description;
            ServerUrl = source.ServerUrl;
            ServerHostname = source.ServerHostname;
            ServerUrl = source.ServerUrl;
            ServerIp = source.ServerIp;
            Level = source.Level;
            LastWiped = source.LastWiped;
            ServerPort = source.ServerPort;
            RconIp = source.RconIp;
            RconPort = source.RconPort;
            RconPassword = source.RconPassword;
            AppPort = source.AppPort;
            RconWeb = source.RconWeb;
            UseCustomMap = source.UseCustomMap;
            MapName = source.MapName;
            MaxPlayers = source.MaxPlayers;
            ServerLevelUrl = source.ServerLevelUrl;
            WorldSize = source.WorldSize;
            Seed = source.Seed;
            RustDedicatedProcess = source.RustDedicatedProcess;
            InstallDirectory = source.InstallDirectory;
            SteamCmdPath = source.SteamCmdPath;
            ServerCfg = source.ServerCfg;
            ServerTickrate = source.ServerTickrate;
            ServerSaveInterval = source.ServerSaveInterval;
            MySqlHost = source.MySqlHost;
            MySqlUserName = source.MySqlUsername;
            MySqlPassword = source.MySqlPassword;
            MySqlPort = source.MySqlPort;
            MySqlDatabaseName = source.MySqlDatabaseName;
            AutoUpdate = source.AutoUpdate;
            AutoStart = source.AutoStart;
            EnableGracefulShutdown = source.EnableGracefulShutdown;
            ShutdownDelaySeconds = source.ShutdownDelaySeconds;
            ShutdownMessageCommand = source.ShutdownMessageCommand;
        }

        public void ApplyTo(RustServerInstance target)
        {
            target.Identity = Identity;
            target.Description = Description;
            target.ServerUrl = ServerUrl;
            target.ServerHostname = ServerHostname;
            target.ServerUrl = ServerUrl;
            target.ServerIp = ServerIp;
            target.Level = Level;
            target.LastWiped = LastWiped;
            target.ServerPort = ServerPort;
            target.RconIp = RconIp;
            target.RconPort = RconPort;
            target.RconPassword = RconPassword;
            target.AppPort = AppPort;
            target.RconWeb = RconWeb;
            target.UseCustomMap = UseCustomMap;
            target.MapName = MapName;
            target.MaxPlayers = MaxPlayers;
            target.ServerLevelUrl = ServerLevelUrl;
            target.WorldSize = WorldSize;
            target.Seed = Seed;
            target.RustDedicatedProcess = RustDedicatedProcess;
            target.InstallDirectory = InstallDirectory;
            target.SteamCmdPath = SteamCmdPath;
            target.ServerCfg = ServerCfg;
            target.ServerTickrate = ServerTickrate;
            target.ServerSaveInterval = ServerSaveInterval;
            target.MySqlHost = MySqlHost;
            target.MySqlUsername = MySqlUserName;
            target.MySqlPassword = MySqlPassword;
            target.MySqlPort = MySqlPort;
            target.MySqlDatabaseName = MySqlDatabaseName;
            target.AutoStart = AutoStart;
            target.AutoUpdate = AutoUpdate;
            target.EnableGracefulShutdown = EnableGracefulShutdown;
            target.ShutdownDelaySeconds = ShutdownDelaySeconds;
            target.ShutdownMessageCommand = ShutdownMessageCommand;
        }
    }
}