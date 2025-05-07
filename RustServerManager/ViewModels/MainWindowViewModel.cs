using RustServerManager.Models;
using System;
using System.Collections.ObjectModel;

namespace RustServerManager.ViewModels
{
    public class MainWindowViewModel : ObservableCollection<MainWindowViewModel>
    {
        public MainWindowViewModel() 
        { 

        }

        public static DatabaseConfig dbconfig = new();
    }
}
