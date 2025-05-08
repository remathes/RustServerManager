using System.Windows;
using System.Windows.Controls;

namespace RustServerManager.Models
{

    /// <summary>
    /// Interaction logic for DatabaseConfigDialog.xaml
    /// </summary>
    public partial class DatabaseConfigDialog : UserControl
    {

        public RustServerInstance Instance { get; private set; }

        public DatabaseConfigDialog(RustServerInstance instance)
        {
            InitializeComponent();
            Instance = instance;
            if (Instance.DatabaseConfig == null)
            {
                Instance.DatabaseConfig = new DatabaseConfig
                {
                    MySqlHost = instance.MySqlHost,
                    MySqlPort = 3306,
                    MySqlUsername = instance.MySqlUsername,
                    MySqlPassword = instance.MySqlPassword,
                    MySqlDatabaseName = instance.Identity
                };
            }

            PasswordBox.Password = Instance.MySqlPassword;
            this.DataContext = Instance;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Instance?.MySqlDatabaseName != null)
                Instance.MySqlPassword = PasswordBox.Password;
        }

        public static bool? ShowDialog(RustServerInstance instance)
        {
            var dialog = new DatabaseConfigDialog(instance);
            return MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "MainDialog").Result as bool?;
        }
    }
}