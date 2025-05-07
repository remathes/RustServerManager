using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RustServerManager.Views
{
    /// <summary>
    /// Interaction logic for RustServerOverviewPage.xaml
    /// </summary>
    public partial class RustServerOverviewPage : UserControl
    {
        public RustServerOverviewPage()
        {
            InitializeComponent();
            Loaded += RustServerOverviewPage_Loaded;
        }

        private void RustServerOverviewPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Storyboard pulse = (Storyboard)FindResource("ForgePulseAnimation");
            //pulse.Begin(ForgeCard, true);
        }
    }
}
