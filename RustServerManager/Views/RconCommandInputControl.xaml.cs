using RustServerManager.Services;
using RustServerManager.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace RustServerManager.Views
{
    public partial class RconCommandInputControl : UserControl
    {
        RconClient rcon = RconClient.Instance;
        string log_name = string.Empty;
        public static RustInstanceGridViewModel SelectedInsatance { get; set; }
        RustInstanceGridViewModel ViewModel = new RustInstanceGridViewModel();
        public RconCommandInputControl()
        {
            InitializeComponent();
            Loaded += RconCommandInputControl_Loaded;
            //QCC.rconCommandInputControl = this;
            this.DataContext = ViewModel;
        }

        public async void SendRconOutput(string cmd)
        {
            if (!string.IsNullOrEmpty(cmd))
            {
                await SendCommandAsync(cmd);
            }
        }

        private void RconCommandInputControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {

            }
        }

        private async Task SendCommandAsync(string command)
        {
            //var config = SelectedInsatance as ServerConfigViewModel;
            //if (config == null)
            //    return;
            //string cmd = command.Trim();
            //if (string.IsNullOrWhiteSpace(cmd)) return;

            
            //if (await rcon.EnsureConnectedAsync(config.RconIp, 28016, config.RconPassword))
            //{
            //    string output = await rcon.SendCommandAsync("status");
            //    Console.WriteLine($"RCON: {output}");
            //}
            //else
            //{
            //    Console.WriteLine("Failed to connect to RCON");
            //}
            //try
            //{
                
            //    if (SelectedInsatance != null)
            //    {

            //        //QCC.SetConnectionStatus(false, true);
            //        ConsoleOutput.AppendText("[RCON] Connecting...");

            //        bool connected = await rcon.EnsureConnectedAsync(
            //        config.RconIp,
            //        ushort.Parse(config.RconPort.ToString()),
            //        config.RconPassword);

            //        if (!connected)
            //        {
            //            ConsoleOutput.AppendText("[RCON] ❌ Failed to connect");
            //            return;
            //        }
            //        //QCC.SetConnectionStatus(connected, false);
            //        ConsoleOutput.AppendText("[RCON] ✅ Connected");
            //    }

            //    ConsoleOutput.AppendText("> " + cmd);
            //    // 🧠 This calls the method from RconClient.cs
            //    string response = await rcon.SendCommandAsync(cmd);

            //    ConsoleOutput.AppendText(response);
            //}
            //catch (Exception ex)
            //{
            //    ConsoleOutput.AppendText("[RCON] Error " + ex.Message);
            //    //QCC.SetConnectionStatus(false, false);
            //}
        }



        public void AppendRconOutput(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            _ = ConsoleOutput.Dispatcher.InvokeAsync(() =>
            {
                string timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
                var run = new Run(timestamp + "[RCON] " + message + "\n")
                {
                    Foreground = Brushes.DeepSkyBlue
                };

                var paragraph = new Paragraph(run) { Margin = new Thickness(0) };
                ConsoleOutput.Document.Blocks.Add(paragraph);
                ConsoleOutput.ScrollToEnd();

                const int maxLines = 1000;
                if (ConsoleOutput.Document.Blocks.Count > maxLines)
                {
                    ConsoleOutput.Document.Blocks.Remove(ConsoleOutput.Document.Blocks.FirstBlock);
                }
            });
        }

        public async Task ConnectRconAsync(string ip, ushort port, string password)
        {
            //QCC (false, true);
            bool success = await rcon.EnsureConnectedAsync(ip, port, password);
            AppendRconOutput(success ? "Connected to RCON." : "Failed to connect to RCON.");
            //QCC.SetConnectionStatus(success, false);
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Document.Blocks.Clear();
        }
    }
}