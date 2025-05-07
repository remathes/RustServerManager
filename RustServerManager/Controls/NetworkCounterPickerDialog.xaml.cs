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
    /// Interaction logic for NetworkCounterPickerDialog.xaml
    /// </summary>
    public partial class NetworkCounterPickerDialog : UserControl
    {
        public class NetworkCounterOption
        {
            public string DisplayName { get; set; }  // e.g., "Intel Wi-Fi (192.168.1.117)"
            public string CounterName { get; set; }  // actual performance counter name
            public string IpAddress { get; set; }     // optional for future logic
        }

        public NetworkCounterPickerDialog()
        {
            InitializeComponent();
        }
    }
}
