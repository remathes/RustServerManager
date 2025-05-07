// MainWindow.xaml.cs - Updated for MaterialDesign 5.x theme handling
using MaterialDesignThemes.Wpf;
using RustServerManager.Controls;
using RustServerManager.ViewModels;
using RustServerManager.Views;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Threading.Tasks;
using static RustServerManager.Controls.NetworkCounterPickerDialog;
using System.Collections.Generic;
using Org.BouncyCastle.Tls;
using RustServerManager.Utils;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Xml.Linq;
using RustServerManager.Models;
using System.IO;
using System.Text.Json;

namespace RustServerManager
{
    public partial class MainWindow : Window
    {
        public static DialogSession Session { get; private set; }
        public void ShowSnackbar(string message)
        {
            //MainSnackbar.MessageQueue?.Enqueue(message);
        }

        

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        public async Task <List<string>> FindMySql(string path)
        {
          return await SafeEnumerateFiles.EnumerateFilesSafeAsync(path, "MySql.exe", SearchOption.AllDirectories);
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string dbconfigjson = File.ReadAllText("dbconfig.json");
            var dbconfig = JsonSerializer.Deserialize<DatabaseConfig>(dbconfigjson);
            MainWindowViewModel.dbconfig = dbconfig;
            //var myssqlinstalled = await MySqlInstaller.DownloadAndLaunchMySqlInstallerAsync(Path.Combine(Path.GetTempPath(), "MySqlDownload"));
            //if (NetworkCounterDetector.TryDetect(out string counter, out string ip))
            //{
            //    NetworkMonitorOverrides.Reset(Environment.MachineName);
            //    NetworkMonitorOverrides.Set(Environment.MachineName, counter, ip);
            //}
            //await Dispatcher.InvokeAsync(async () => 
            //{
            //    List<string> mysqlpaths = new List<string>();
            //    var drives = System.IO.DriveInfo.GetDrives();
            //    if (drives.Length > 0)
            //    {
            //        foreach(var drive in drives)
            //        {
            //            if (drive.IsReady)
            //            {
            //                if (drive.DriveType == DriveType.Fixed)
            //                {
            //                    List<string> files = await Task.Run(() => FindMySql(drive.RootDirectory.FullName));
            //                    if (files != null)
            //                    {
            //                        foreach (var file in files) {mysqlpaths.Add(file); }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    if (mysqlpaths.Count > 0)
            //    {
            //        { 
            //            foreach (var file in mysqlpaths)
            //            {
            //                if (!File.Exists("MySqlInstalls.txt"))
            //                {
            //                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(file);
            //                    string filedetails = $"{file},{fileVersionInfo}";
            //                    await File.AppendAllTextAsync("MySqlInstalls.txt",filedetails);
            //                }
            //            } 
            //        }
            //    }
            //    else
            //    {
            //        await File.AppendAllTextAsync("","Not Installed,Unknown");
            //    }
            //});
        }
    }
}
