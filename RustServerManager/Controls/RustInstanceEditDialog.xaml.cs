// RustInstanceEditDialog.xaml.cs
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using RustServerManager.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace RustServerManager.Controls
{
    public partial class RustInstanceEditDialog : UserControl
    {
        public RustInstanceEditDialog()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        RustInstanceEditViewModel ViewModel = new RustInstanceEditViewModel();
        
        private const string SafeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
        private static string GenerateSafeRconPassword(int length = 12)
        {
            if (length < 8) throw new ArgumentException("RCON password should be at least 8 characters.");

            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[length];
            rng.GetBytes(buffer);

            return new string(buffer.Select(b => SafeChars[b % SafeChars.Length]).ToArray());
        }

        private void GenerateRconPassword_Click(object sender, RoutedEventArgs e)
        {
            TextBoxRconPassword.Text = GenerateSafeRconPassword();
            ViewModel.RconPassword = TextBoxRconPassword.Text;
        }

        private void OnBrowseSteamCmdClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Select or Create a SteamCMD install folder where steamcmd.exe will be installed";

            if (dialog.ShowDialog() == true && DataContext is RustInstanceEditViewModel vm)
            {
                vm.SteamCmdPath = dialog.SelectedPath;
            }
        }

        private void OnBrowseInstallDirClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Select or Create a Install folder where you're rust server will be installed";

            if (dialog.ShowDialog() == true && DataContext is RustInstanceEditViewModel vm)
            {
                vm.InstallDirectory = dialog.SelectedPath;
                vm.ServerCfg = $"{dialog.SelectedPath}\\server\\{vm.Identity}\\cfg\\server.cfg";
                vm.RustDedicatedProcess = Path.Combine(vm.InstallDirectory, "RustDedicated.exe");
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}