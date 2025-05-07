using MaterialDesignThemes.Wpf;
using RustUpdate;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Timers;
using System.Windows.Forms;
using System.Text.Json;
using System.Windows.Shapes;

namespace RustUpdate
{
    public partial class MainWindow : Window
    {
        private AppSettings? settings;
        private RustUpdater? updater = null;
        private readonly PaletteHelper _paletteHelper = new();
        private System.Timers.Timer? updateCheckTimer;
        public System.Windows.Forms.NotifyIcon? trayicon;
        public MainWindow()
        {
            try
            {
                settings = AppSettings.Load();
            }
            catch
            {
                settings = new AppSettings();
                var settingsWindow = new SettingsWindow(settings);
                var dialog = settingsWindow.ShowDialog();
                if (dialog != true)
                {
                    if (string.IsNullOrEmpty(settings.RustInstallPath) && string.IsNullOrEmpty(settings.SteamCMDPath))
                    {
                        System.Windows.MessageBox.Show("Install / Update unable to continue without install paths...");
                        this.Close();
                    }
                }
            }
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 3)
            {
                if (Directory.Exists(args[1]) && Directory.Exists(args[2]))
                {
                    settings.SteamCMDPath = args[1];
                    settings.RustInstallPath = args[2];
                    string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText("AppSettings.json", json);
                }
            }
            if (!string.IsNullOrEmpty(settings.SteamCMDPath) && !string.IsNullOrEmpty(settings.RustInstallPath))
            {
                updater = new RustUpdater(settings.SteamCMDPath, settings.RustInstallPath,settings.SteamCmdExePath);
                updater.HandleMinimize(this);
                if (settings != null)
                {
                    if (settings.EnableAutoUpdateCheck)
                    {
                        ScheduleDailyUpdateCheck();
                        StartPeriodicUpdateChecks();
                    }
                }
                LoadVersions();
            }
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
            else if(WindowState == WindowState.Normal)
            {
                this.ShowInTaskbar = true;
            }
        }
        private async void ScheduleDailyUpdateCheck()
        {
            if (settings != null)
            {
                if (settings.EnableAutoUpdateCheck)
                {
                    if (settings.LastUpdateCheck == null && settings.EnableAutoUpdateCheck || settings.LastUpdateCheck.Value.Date < DateTime.Today && settings.EnableAutoUpdateCheck)
                    {
                        var (installedRust, latestRust) = await updater.GetRustVersionsAsync();
                        var installedOxide = updater.GetInstalledOxideVersion();
                        var latestOxide = await updater.GetLatestOxideVersionAsync();

                        bool rustNeedsUpdate = installedRust != "Not found" && !string.IsNullOrEmpty(latestRust) && string.Compare(installedRust, latestRust, true) < 0;
                        bool oxideNeedsUpdate = installedOxide != "Not found" && !string.IsNullOrEmpty(latestOxide) && string.Compare(installedOxide, latestOxide, true) < 0;

                        if (rustNeedsUpdate || oxideNeedsUpdate)
                        {
                            string message = "Update available for " +
                                             (rustNeedsUpdate ? "Rust " : "") +
                                             (oxideNeedsUpdate ? "Oxide" : "");
                            updater.NotifyUpdateAvailable(message.Trim());
                        }

                        settings.LastUpdateCheck = DateTime.Now;
                        settings.Save();
                    }
                }
            }
        }

        private void StartPeriodicUpdateChecks()
        {
            updateCheckTimer = new System.Timers.Timer
            {
                Interval = TimeSpan.FromHours(3).TotalMilliseconds,
                AutoReset = true,
                Enabled = true
            };

            updateCheckTimer.Elapsed += async (s, e) =>
            {
                if (settings.EnableAutoUpdateCheck && settings.ShouldCheckForUpdates())
                {
                    var (installedRust, latestRust) = await updater.GetRustVersionsAsync();
                    var installedOxide = updater.GetInstalledOxideVersion();
                    var latestOxide = await updater.GetLatestOxideVersionAsync();

                    bool rustNeedsUpdate = installedRust != "Not found" && latestRust != null && string.Compare(installedRust, latestRust, true) < 0;
                    bool oxideNeedsUpdate = installedOxide != "Not found" && latestOxide != null && string.Compare(installedOxide, latestOxide, true) < 0;

                    if (rustNeedsUpdate || oxideNeedsUpdate)
                    {
                        string message = "Update available for " +
                                         (rustNeedsUpdate ? "Rust " : "") +
                                         (oxideNeedsUpdate ? "Oxide" : "");
                        updater.NotifyUpdateAvailable(message.Trim());
                    }

                    settings.MarkUpdateChecked();
                }
            };
        }

        private void ApplyTheme(bool isDark)
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            _paletteHelper.SetTheme(theme);
        }

        private void DarkModeToggle_Changed(object sender, RoutedEventArgs e)
        {
            IsDarkMode = DarkModeToggle.IsChecked == true;
        }

        public bool IsDarkMode
        {
            get => Properties.Settings.Default.IsDarkMode;
            set
            {
                Properties.Settings.Default.IsDarkMode = value;
                Properties.Settings.Default.Save();
                ApplyTheme(value);
            }
        }

        public async void LoadVersions()
        {
            var (installedRust, latestRust) = await updater.GetRustVersionsAsync();
            var installedOxide = updater.GetInstalledOxideVersion();
            var latestOxide = await updater.GetLatestOxideVersionAsync();

            bool rustInstalled = installedRust != "Not Found";
            bool rustNeedsUpdate = rustInstalled && string.Compare(installedRust, latestRust, true) < 0;




            // Installed Version Update labels
            RustCurrentVersionLabel.Text = rustInstalled ? (rustNeedsUpdate ? $"Rust Update Available: {installedRust}" : $"Rust is Up To Date: {installedRust}") : "Install Rust";
            RustCurrentVersionLabel.Foreground = rustInstalled ? (rustNeedsUpdate ? Brushes.Red : Brushes.LimeGreen) : Brushes.Orange;

            // Online Check Update labels
            RustVersionLabel.Text = latestRust != null ? $"Current Rust Version Available: {latestRust}" : "Current Rust Version Available: Unknown";
            RustVersionLabel.Foreground = latestRust !=null? Brushes.LimeGreen : Brushes.Orange;

            //Button Update / Up To Date / Install
            UpdateRustButton.Content = rustInstalled ? (rustNeedsUpdate ? "Update Rust" : "Rust Up To Date") : "Install Rust";
            //Enable Button if Not Fount or Needs Update
            UpdateRustButton.IsEnabled = !rustInstalled || rustNeedsUpdate;



            bool oxideInstalled = installedOxide != "Not Found";
            bool oxideNeedsUpdate = oxideInstalled && string.Compare(installedOxide, latestOxide, true) < 0;

            // Installed Version Update labels
            OxideCurrentVersionLabel.Text = oxideInstalled ? (oxideNeedsUpdate ? $"Oxide Update Available: {installedOxide}" : $"Oxide is Up To Date {installedOxide}") : "Install Oxide";
            OxideCurrentVersionLabel.Foreground = oxideInstalled ? (oxideNeedsUpdate ? Brushes.Red : Brushes.LimeGreen) : Brushes.Orange;

            // Online Check Update labels
            OxideVersionLabel.Text = latestOxide != null ? $"Current Oxide Version Available: {latestOxide}" : "Current Oxide Version Available: Unknown";
            OxideVersionLabel.Foreground = latestOxide != null ? Brushes.LimeGreen : Brushes.Orange;
            
            //Button Update / Up To Date / Install
            UpdateOxideButton.Content = oxideInstalled ? (oxideNeedsUpdate ? "Update Oxide" : "Oxide Up To Date") : "Install Oxide";
            //Enable Button if Not Fount or Needs Update
            UpdateOxideButton.IsEnabled = !oxideInstalled || oxideNeedsUpdate;
        }

        public async void UpdateRust_Click(object sender, RoutedEventArgs e)
        {
            if (settings != null)
            {
                if (settings.SteamCMDPath != null)
                {
                    if (updater != null)
                    {
                        RustStatusLabel.Text = "Starting steamcmd install...";
                        bool success = await Task.Run(async () =>
                        {
                            return await updater.InstallSteamCMDAsync(RustStatusLabel,this,settings.SteamCMDPath);
                        });
                        if(!success)
                        {
                            return;
                        }    
                    }
                }
                RustStatusLabel.Text = "Please wait installing / updating rust...";
                await Task.Run(() => updater.UpdateRustServer(RustStatusLabel, this));
            }
        }

        private async void UpdateOxide_Click(object sender, RoutedEventArgs e)
        {
            RustStatusLabel.Text = "Starting...";
            await Task.Run(() => updater.UpdateOxide(RustStatusLabel,this));
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(settings);
            if (settingsWindow.ShowDialog() == true)
            {
                settings.Save();
                updater = new RustUpdater(settings.SteamCMDPath, settings.RustInstallPath,settings.SteamCmdExePath);
                LoadVersions();
            }
        }
    }
}