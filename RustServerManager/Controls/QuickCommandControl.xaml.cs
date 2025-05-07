// QuickCommandControl.xaml.cs
using MaterialDesignThemes.Wpf;
using RustServerManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RustServerManager.Controls
{
    public partial class QuickCommandControl : UserControl
    {
        public ObservableCollection<QuickCommand> AllCommands { get; set; } = new();
        public ObservableCollection<QuickCommand> FilteredCommands { get; set; } = new();
        private void FilterCommands()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCommands.Clear();
                return;
            }

            var results = AllCommands
                .Where(cmd => cmd.Command.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(cmd => cmd.Command)
                .ToList();

            FilteredCommands.Clear();
            foreach (var cmd in results)
            {
                FilteredCommands.Add(cmd);
            }

            IsPopupOpen = FilteredCommands.Any(); // assuming you have this property
        }

        public QuickCommand SelectedCommand { get; set; }

        public Action<string> CommandSent { get; set; }
        public bool IsPopupOpen
        {
            get => (bool)GetValue(IsPopupOpenProperty);
            set => SetValue(IsPopupOpenProperty, value);
        }

        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register(nameof(IsPopupOpen), typeof(bool), typeof(QuickCommandControl),
                new PropertyMetadata(false));
        public static RconCommandInputControl rconCommandInputControl { get; set; }
        
        public Brush Green = Brushes.LimeGreen;
        public Brush Crimsion = Brushes.Crimson;
        public bool IsConnected { get; set; }

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register(nameof(IsConnected), typeof(bool), typeof(QuickCommandControl),
                new PropertyMetadata(false));
        public bool IsConnecting { get; set; }
        public static readonly DependencyProperty IsConnectingProperty =
            DependencyProperty.Register(nameof(IsConnecting), typeof(bool), typeof(QuickCommandControl),
                new PropertyMetadata(false));
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(QuickCommandControl),
                new PropertyMetadata(true, OnIsExpandedChanged));
        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is QuickCommandControl control)
            {
                control.UpdateExpandState();
            }
        }
        public IEnumerable<QuickCommand> ItemsSource
        {
            get => (IEnumerable<QuickCommand>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<QuickCommand>), typeof(QuickCommandControl),
                new PropertyMetadata(null));
        public QuickCommand SelectedItem
        {
            get => (QuickCommand)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(QuickCommand), typeof(QuickCommandControl),
                new PropertyMetadata(null));

        private void UpdateExpandState()
        {
            ExpandIcon.Kind = IsExpanded ? PackIconKind.ChevronDown : PackIconKind.ChevronUp;
            DescriptionContainer.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        public class QuickCommand
        {
            public string Command { get; set; }
            public string Description { get; set; }
            public override string ToString() => Command;
        }

        public QuickCommandControl()
        {
            InitializeComponent();
            // ✅ Set the whole control's DataContext to itself
            this.DataContext = this;
            _ = LoadCommandsFromJsonAsync("QuickCommands.json");
        }

        public async Task LoadCommandsFromJsonAsync(string path)
        {
            if (!File.Exists(path))
                return;

            try
            {
                string json = await File.ReadAllTextAsync(path);
                var items = JsonSerializer.Deserialize<List<QuickCommand>>(json);

                if (items == null)
                    return;

                // Use dispatcher if updating UI-bound ObservableCollection
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AllCommands.Clear();
                    foreach (var item in items)
                    {
                        AllCommands.Add(new QuickCommand
                        {
                            Command = item.Command,
                            Description = item.Description
                        });
                    }

                    // Sort into filtered list if needed
                    FilteredCommands.Clear();
                    foreach (var sorted in AllCommands.OrderBy(a => a.Command))
                    {
                        FilteredCommands.Add(sorted);
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle file or deserialization errors if needed
                Debug.WriteLine("Error loading commands: " + ex.Message);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CleatButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleExpand_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;

            // Swap the icon: down when expanded, up when collapsed
            ExpandIcon.Kind = IsExpanded ? PackIconKind.ChevronDown : PackIconKind.ChevronUp;

            // Toggle description container visibility
            DescriptionContainer.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        }
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(QuickCommandControl),
                new PropertyMetadata(string.Empty, OnSearchTextChanged));

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is QuickCommandControl control)
            {
                control.FilterCommands(); // Update the list when text changes
            }
        }

        private void ListBoxList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
