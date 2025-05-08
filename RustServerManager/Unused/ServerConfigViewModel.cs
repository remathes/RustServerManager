// ViewModels/ServerConfigViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.VisualElements;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Timers;
namespace RustServerManager.ViewModels
{
    public partial class ServerConfigViewModel : ObservableObject
    {


        //[ObservableProperty] private string consoleHeader = "";
        //[ObservableProperty] private string serverOutput = "";
        //[ObservableProperty] public double installProgress = 0;
        //[ObservableProperty] public bool isInstalling = false;
        //[ObservableProperty] public string installStatusMessage = "";
        //[ObservableProperty] private bool isConnecting = false;
        //[ObservableProperty] private bool isConnected = false;
        private System.Timers.Timer _monitorTimer;
        private Process _rustProcess;
        public ServerConfigViewModel()
        {

        }


        //public RustInstanceGridViewModel.ServerConfig Config { get; }

        //public ServerConfigViewModel(RustInstanceGridViewModel.ServerConfig config)
        //{
        //    Config = config;
        //    TryAttachToRunningProcess();
        //    UpdateStatus();
        //}



        //public string Identity
        //{
        //    get => Config.Identity;
        //    set { Config.Identity = value; OnPropertyChanged(); }
        //}

        //public string ServerHostname
        //{
        //    get => Config.ServerHostname;
        //    set { Config.ServerHostname = value; OnPropertyChanged(); }
        //}

        //public string Description
        //{
        //    get => Config.Description;
        //    set { Config.Description = value; OnPropertyChanged(); }
        //}

        //public string ServerIp
        //{
        //    get => Config.ServerIp;
        //    set { Config.ServerIp = value; OnPropertyChanged(); }
        //}

        //public int ServerPort
        //{
        //    get => Config.ServerPort;
        //    set { Config.ServerPort = value; OnPropertyChanged(); }
        //}

        //public string RconIp
        //{
        //    get => Config.RconIp;
        //    set { Config.RconIp = value; OnPropertyChanged(); }
        //}

        //public int RconPort
        //{
        //    get => Config.RconPort;
        //    set { Config.RconPort = value; OnPropertyChanged(); }
        //}

        //public string RconPassword
        //{
        //    get => Config.RconPassword;
        //    set { Config.RconPassword = value; OnPropertyChanged(); }
        //}

        //public int RconWeb
        //{
        //    get => Config.RconWeb;
        //    set { Config.RconWeb = value; OnPropertyChanged(); }
        //}

        //public int MaxPlayers
        //{
        //    get => Config.MaxPlayers;
        //    set { Config.MaxPlayers = value; OnPropertyChanged(); }
        //}

        //public int ServerTickrate
        //{
        //    get => Config.ServerTickrate;
        //    set { Config.ServerTickrate = value; OnPropertyChanged(); }
        //}

        //public int ServerSaveInterval
        //{
        //    get => Config.ServerSaveInterval;
        //    set { Config.ServerSaveInterval = value; OnPropertyChanged(); }
        //}

        //public bool UseCustomMap
        //{
        //    get => Config.UseCustomMap;
        //    set { Config.UseCustomMap = value; OnPropertyChanged(); }
        //}

        //public int Seed
        //{
        //    get => Config.Seed;
        //    set { Config.Seed = value; OnPropertyChanged(); }
        //}

        //public int WorldSize
        //{
        //    get => Config.WorldSize;
        //    set { Config.WorldSize = value; OnPropertyChanged(); }
        //}

        //public string MapName
        //{
        //    get => Config.MapName;
        //    set { Config.MapName = value; OnPropertyChanged(); }
        //}

        //public string ServerLevelUrl
        //{
        //    get => Config.ServerLevelUrl;
        //    set { Config.ServerLevelUrl = value; OnPropertyChanged(); }
        //}

        //public string SteamCmdPath
        //{
        //    get => Config.SteamCmdPath;
        //    set { Config.SteamCmdPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanInstallRust)); }
        //}

        //public string InstallDirectory
        //{
        //    get => Config.InstallDirectory;
        //    set { Config.InstallDirectory = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanInstallRust)); }
        //}

        //public string RustDedicatedProcess
        //{
        //    get => Config.RustDedicatedProcess;
        //    set { Config.RustDedicatedProcess = value; OnPropertyChanged(); }
        //}

    }
}