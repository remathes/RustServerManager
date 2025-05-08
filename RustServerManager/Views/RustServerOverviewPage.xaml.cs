using System.Windows.Controls;

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

        }
    }
}
