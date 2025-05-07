using RustServerManager.Utils;
using System.Windows;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;

namespace RustServerManager.Controls
{
    /// <summary>
    /// Interaction logic for PreInstall.xaml
    /// </summary>
    public partial class PreInstall : Window
    {
        public PreInstall()
        {
            InitializeComponent();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            TBYes.Text = "Please wait...";
            BtnYes.IsEnabled = false;
            var installed = await MySqlInstaller.DownloadAndLaunchMySqlInstallerAsync(Path.Combine(Path.GetTempPath(),"MySqlDownload"));
            if(installed)
            {
                MessageBox.Show("Installation Complete.");
                this.Close();
            }
        }
    }
}
