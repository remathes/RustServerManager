using System.Windows.Controls;

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
