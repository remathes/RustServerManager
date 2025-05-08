using System.Windows.Controls;

namespace RustServerManager.Controls
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : UserControl
    {
        public string Message { get; }

        public ProgressDialog(string message)
        {
            Message = message;
            InitializeComponent();
            DataContext = this;
        }
    }
}
