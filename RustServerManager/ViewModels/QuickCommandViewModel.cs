using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace RustServerManager.ViewModels
{
    public partial class QuickCommandViewModel : ObservableObject, INotifyPropertyChanged
    {
        public ObservableCollection<QuickCommand> AllCommands { get; set; } = new();
        public ObservableCollection<QuickCommand> FilteredCommands { get; set; } = new();
        public class QuickCommand
        {
            public string Command { get; set; }
            public string Description { get; set; }
            public override string ToString() => Command;
        }

        public Action<string> CommandSent { get; set; }

        public QuickCommandViewModel()
        {
            LoadCommandsFromJson("QuickCommands.json");
        }

        private void LoadCommandsFromJson(string path)
        {
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<QuickCommand>>(json);
            
        }
    }
}