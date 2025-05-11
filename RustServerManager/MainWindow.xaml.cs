// MainWindow.xaml.cs - Updated for MaterialDesign 5.x theme handling
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using RustServerManager.Controls;
using RustServerManager.Models;
using RustServerManager.Utils;
using RustServerManager.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RustServerManager
{
    public partial class MainWindow : Window
    {
        public static DialogSession Session { get; private set; }
        public void ShowSnackbar(string message)
        {
            //MainSnackbar.MessageQueue?.Enqueue(message);
        }


        public RustInstanceGridViewModel ViewModel { get; } = new();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            this.DataContext = ViewModel;
        }


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string dbconfigjson = File.ReadAllText("dbconfig.json");
            var dbconfig = JsonSerializer.Deserialize<DatabaseConfig>(dbconfigjson);
            MainWindowViewModel.dbconfig = dbconfig;
            await Dispatcher.InvokeAsync(async () =>
            {
                var dialog = new ScanDialog(); // your UserControl
                var result = await DialogHost.Show(dialog, "MainDialog") as DialogSession;
            }, System.Windows.Threading.DispatcherPriority.Background);
            await ViewModel.LoadInstances();
            DialogHost.Close("MainDialog");

            //Cuurent Selected
            if (ViewModel.Instances.Any())
                ViewModel.SelectedInstance = ViewModel.Instances.First();
            ViewModel.EditCommand.NotifyCanExecuteChanged();
            ViewModel.BackInstanceCommand.NotifyCanExecuteChanged();
            ViewModel.NextInstanceCommand.NotifyCanExecuteChanged();
            var rotateStoryboard = (Storyboard)FindResource("RotateBoltAnimation");
            rotateStoryboard.Begin();
        }
    }
}
