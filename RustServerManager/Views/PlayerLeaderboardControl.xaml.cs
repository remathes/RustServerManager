using RustServerManager.ViewModels;
using System.Windows.Controls;

namespace RustServerManager.Views
{
    /// <summary>
    /// Interaction logic for PlayerLeaderboardControl.xaml
    /// </summary>
    public partial class PlayerLeaderboardControl : UserControl
    {
        public PlayerStatsTabViewModel ViewModel { get; set; }
        public PlayerLeaderboardControl()
        {
            InitializeComponent();
            ViewModel = new PlayerStatsTabViewModel();
            this.DataContext = ViewModel;
        }
    }
}
