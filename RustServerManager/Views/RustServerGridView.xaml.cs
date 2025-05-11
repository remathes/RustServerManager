using MaterialDesignThemes.Wpf;
using RustServerManager.Controls;
using RustServerManager.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace RustServerManager.Views
{
    /// <summary>
    /// Interaction logic for RustServerGridView.xaml
    /// </summary>
    public partial class RustServerGridView : UserControl
    {
        public RustInstanceGridItemViewModel ViewModel { get; }
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

        private void RustServerGridView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var rotateStoryboard = (Storyboard)FindResource("RotateBoltAnimation");
            rotateStoryboard.Begin();
        }
    }
}
