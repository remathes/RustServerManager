using System.Windows.Controls;
using System.Windows.Input;

namespace RustServerManager.Controls
{
    /// <summary>
    /// Interaction logic for BredcrumbSegment.xaml
    /// </summary>
    public partial class BredcrumbSegment : UserControl
    {
        public BredcrumbSegment()
        {
            InitializeComponent();
        }
    }

    public class BreadcrumbSegment
    {
        public string Name { get; set; }
        public ICommand NavigateCommand { get; set; }
    }
}
