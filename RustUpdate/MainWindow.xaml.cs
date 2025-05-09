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
using System.Windows.Media.TextFormatting;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace RustUpdate
{
    public partial class MainWindow : Window
    {
        private AppSettings? settings;
        private RustUpdater? updater = null;
        private readonly PaletteHelper _paletteHelper = new();
        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
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
            OverlayGrid.Visibility = Visibility.Hidden;
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
                updater = new RustUpdater(settings.SteamCMDPath, settings.RustInstallPath, settings.SteamCmdExePath);
                LoadVersions();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var rotateStoryboard = (Storyboard)FindResource("RotateBoltAnimation");
            rotateStoryboard.Begin();
        }

        public async void LoadVersions()
        {
            OverlayGrid.Visibility = Visibility.Visible;

            var (installedRust, latestRust) = await updater.GetRustVersionsAsync();
            var installedOxide = updater.GetInstalledOxideVersion();
            var latestOxide = await updater.GetLatestOxideVersionAsync();

            // Neutral consistent text color

            // === RUST ===
            bool rustInstalled = installedRust != "Not Found";
            bool rustNeedsUpdate = rustInstalled && string.Compare(installedRust, latestRust, true) < 0;

            RustCurrentVersionLabel.Text = rustInstalled
                ? (rustNeedsUpdate ? $"Rust Update: {installedRust}" : $"Rust is Up To Date: {installedRust}")
                : "Install Rust";

            RustVersionLabel.Text = latestRust != null
                ? $"Rust Version: {latestRust}" : "Rust Version: Unknown";

            RustStatusIcon.Kind = !rustInstalled
                ? PackIconKind.CloudDownload
                : rustNeedsUpdate
                    ? PackIconKind.Update
                    : PackIconKind.CheckCircle;
            RustStatusIcon.Foreground = !rustInstalled
                ? Brushes.Orange
                : rustNeedsUpdate
                    ? Brushes.OrangeRed
                    : Brushes.Green;

            UpdateRustButton.Content = rustInstalled
                ? (rustNeedsUpdate ? "Update Rust" : "Rust Up To Date")
                : "Install Rust";
            UpdateRustButton.IsEnabled = !rustInstalled || rustNeedsUpdate;

            // === OXIDE ===
            bool oxideInstalled = installedOxide != "Not Found";
            bool oxideNeedsUpdate = oxideInstalled && string.Compare(installedOxide, latestOxide, true) < 0;

            OxideCurrentVersionLabel.Text = oxideInstalled
                ? (oxideNeedsUpdate ? $"Oxide Update: {installedOxide}" : $"Oxide is Up To Date: {installedOxide}")
                : "Install Oxide";

            OxideVersionLabel.Text = latestOxide != null
                ? $"Oxide Version: {latestOxide}" : "Oxide Version: Unknown";

            OxideStatusIcon.Kind = !oxideInstalled
                ? PackIconKind.CloudDownload
                : oxideNeedsUpdate
                    ? PackIconKind.Update
                    : PackIconKind.CheckCircle;
            OxideStatusIcon.Foreground = !oxideInstalled
                ? Brushes.Orange
                : oxideNeedsUpdate
                    ? Brushes.OrangeRed
                    : Brushes.Green;

            UpdateOxideButton.Content = oxideInstalled
                ? (oxideNeedsUpdate ? "Update Oxide" : "Oxide Up To Date")
                : "Install Oxide";
            UpdateOxideButton.IsEnabled = !oxideInstalled || oxideNeedsUpdate;

            OverlayGrid.Visibility = Visibility.Hidden;
        }

        private BitmapImage LoadIcon(string filename)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/RustServerManager;component/Assets/{filename}", UriKind.Absolute));
        }
        //public async void LoadVersions()
        //{
        //    OverlayGrid.Visibility = Visibility.Visible;
        //    var (installedRust, latestRust) = await updater.GetRustVersionsAsync();
        //    var installedOxide = updater.GetInstalledOxideVersion();
        //    var latestOxide = await updater.GetLatestOxideVersionAsync();

        //    bool rustInstalled = installedRust != "Not Found";
        //    bool rustNeedsUpdate = rustInstalled && string.Compare(installedRust, latestRust, true) < 0;

        //    // Installed Version Update labels
        //    RustCurrentVersionLabel.Text = rustInstalled ? (rustNeedsUpdate ? $"Rust Update Available: {installedRust}" : $"Rust is Up To Date: {installedRust}") : "Install Rust";
        //    RustCurrentVersionLabel.Foreground = rustInstalled ? (rustNeedsUpdate ? Brushes.Red : Brushes.LimeGreen) : Brushes.Orange;

        //    // Online Check Update labels
        //    RustVersionLabel.Text = latestRust != null ? $"Rust Version Available: {latestRust}" : "Rust Version Available: Unknown";
        //    RustVersionLabel.Foreground = latestRust != null ? Brushes.LimeGreen : Brushes.Orange;

        //    //Button Update / Up To Date / Install
        //    UpdateRustButton.Content = rustInstalled ? (rustNeedsUpdate ? "Update Rust" : "Rust Up To Date") : "Install Rust";
        //    //Enable Button if Not Fount or Needs Update
        //    UpdateRustButton.IsEnabled = !rustInstalled || rustNeedsUpdate;

        //    bool oxideInstalled = installedOxide != "Not Found";
        //    bool oxideNeedsUpdate = oxideInstalled && string.Compare(installedOxide, latestOxide, true) < 0;

        //    // Installed Version Update labels
        //    OxideCurrentVersionLabel.Text = oxideInstalled ? (oxideNeedsUpdate ? $"Oxide Update Available: {installedOxide}" : $"Oxide is Up To Date {installedOxide}") : "Install Oxide";
        //    OxideCurrentVersionLabel.Foreground = oxideInstalled ? (oxideNeedsUpdate ? Brushes.Red : Brushes.LimeGreen) : Brushes.Orange;

        //    // Online Check Update labels
        //    OxideVersionLabel.Text = latestOxide != null ? $"Oxide Version Available: {latestOxide}" : "Oxide Version Available: Unknown";
        //    OxideVersionLabel.Foreground = latestOxide != null ? Brushes.LimeGreen : Brushes.Orange;

        //    //Button Update / Up To Date / Install
        //    UpdateOxideButton.Content = oxideInstalled ? (oxideNeedsUpdate ? "Update Oxide" : "Oxide Up To Date") : "Install Oxide";
        //    //Enable Button if Not Fount or Needs Update
        //    UpdateOxideButton.IsEnabled = !oxideInstalled || oxideNeedsUpdate;
        //    OverlayGrid.Visibility = Visibility.Hidden;
        //}

        public async void UpdateRust_Click(object sender, RoutedEventArgs e)
        {
            if (settings != null)
            {
                if (settings.SteamCMDPath != null)
                {
                    if (updater != null)
                    {
                        OverlayGrid.Visibility = Visibility.Visible;
                        RustStatusLabel.Text = "Starting steamcmd install...";
                        bool success = await Task.Run(async () =>
                        {
                            return await updater.InstallSteamCMDAsync(RustStatusLabel,this,settings.SteamCMDPath);
                        });
                        if(!success)
                        {
                            return;
                        }
                        OverlayGrid.Visibility = Visibility.Hidden;
                    }
                }
                OverlayGrid.Visibility = Visibility.Visible;
                RustStatusLabel.Text = "Please wait installing / updating rust...";
                await Task.Run(() => updater.UpdateRustServer(RustStatusLabel, this));
                OverlayGrid.Visibility = Visibility.Hidden;
            }
        }

        private async void UpdateOxide_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Visible;
            RustStatusLabel.Text = "Starting...";
            await Task.Run(() => updater.UpdateOxide(RustStatusLabel,this));
            OverlayGrid.Visibility = Visibility.Hidden;
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