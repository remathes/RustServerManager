using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace RustUpdate
{
    public partial class SettingsWindow : Window
    {
        private AppSettings settings;

        public SettingsWindow(AppSettings appSettings)
        {
            InitializeComponent();
            this.settings = appSettings;

            SteamPathBox.Text = settings.SteamCMDPath;
            RustPathBox.Text = settings.RustInstallPath;
        }

        private void BrowseSteam_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SteamPathBox.Text = dlg.SelectedPath;
        }

        private void BrowseRust_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                RustPathBox.Text = dlg.SelectedPath;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(SteamPathBox.Text))
            {
                System.Windows.MessageBox.Show("Please select a folder to install SteamCMD.exe (This is the application used to install a Rust Server)");
                return;
            }
            if(string.IsNullOrEmpty(RustPathBox.Text))
            {
                System.Windows.MessageBox.Show("Please select a folder to install you're Rust Server (x:\\RustServer)");
                return;
            }
            string path = "AppSettings.json";
            bool result;
            settings.SteamCMDPath = SteamPathBox.Text;
            settings.RustInstallPath = RustPathBox.Text;
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
