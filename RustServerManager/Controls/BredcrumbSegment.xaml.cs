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
