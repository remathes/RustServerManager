using RustServerManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            //ViewModel.LoadPlayers();
        }
    }
}
