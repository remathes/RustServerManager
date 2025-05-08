// RustInstanceGrid.xaml.cs
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RustServerManager.ViewModels;
using System.Threading.Tasks;
using RustServerManager.Utils;
using System.Text.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static RustServerManager.ViewModels.RustInstanceGridViewModel;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using RustServerManager.Views;

namespace RustServerManager.Controls
{
    public partial class RustInstanceGrid : UserControl
    {
        public RustInstanceGridViewModel ViewModel { get; } = new();
        private RustServerStatsViewModel _activeStatsViewModel;
        private Process _rustProcess;
        public RustInstanceGrid()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = ViewModel;
                Loaded += (_, _) =>
                {
                    Dispatcher.BeginInvoke(async () =>
                    {
                        await ViewModel.LoadInstances();
                        DialogHost.Close("MainDialog");
                        if(MainWindow.Session != null) 
                        {
                            MainWindow.Session.Close();     
                        }
                    });
                };
            }
        }

        public event Action<RustServerStatsViewModel> StatsViewModelChanged;

        private void RustInstanceGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void InstanceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstanceGrid.SelectedItem is ServerConfigViewModel selected)
            {
                
                //Set instance on rcon control
                RconCommandInputControl.SelectedInsatance = selected;


                // Stop the previous monitor if needed
                //_activeStatsViewModel?._monitorTimer?.Stop(); // if you make it public, or expose StopMonitoring()

                // Switch context
                var vm = new RustServerStatsViewModel { Instance = selected };
                vm.Start();
                StatsViewModelChanged?.Invoke(vm);
            }
        }
    }
}
