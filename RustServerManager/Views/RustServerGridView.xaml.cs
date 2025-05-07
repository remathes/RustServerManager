using MaterialDesignThemes.Wpf;
using RustServerManager.Controls;
using RustServerManager.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace RustServerManager.Views
{
    /// <summary>
    /// Interaction logic for RustServerGridView.xaml
    /// </summary>
    public partial class RustServerGridView : UserControl
    {
        private readonly PaletteHelper _paletteHelper = new();
        public RustServerStatsViewModel StatsViewModel { get; } = new();
        public RustInstanceGridViewModel ViewModel { get; } = new();
        private Process _rustProcess;
        public RustServerGridView()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += RustServerGridView_Loaded;
                this.DataContext = ViewModel;
            }
        }

        public bool IsDarkMode
        {
            get => Properties.Settings.Default.IsDarkMode;
            set
            {
                Properties.Settings.Default.IsDarkMode = value;
                Properties.Settings.Default.Save();
                ApplyTheme(value);
            }
        }

        private void ApplyTheme(bool isDark)
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            _paletteHelper.SetTheme(theme);
        }

        private void DarkModeToggle_Changed(object sender, RoutedEventArgs e)
        {
            //IsDarkMode = DarkModeToggle.IsChecked == true;
        }

        private async void RustServerGridView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Storyboard pulse = (Storyboard)FindResource("ForgePulseAnimation");
            //pulse.Begin(ForgeCard, true);
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
        }
    }
}
