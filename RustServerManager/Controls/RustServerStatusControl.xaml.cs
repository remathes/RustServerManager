using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for RustServerStatusControl.xaml
    /// </summary>
    public partial class RustServerStatusControl : UserControl
    {
        public RustServerStatusControl()
        {
            InitializeComponent();
        }

        //public void UpdateGauges(double cpuPercent, double ramUsedGb, double ramTotalGb)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        PCPU.Value = cpuPercent;
        //        PRAM.Value = ramUsedGb;
        //        PRAM.Maximum = ramTotalGb;

        //        CpuLabel.Text = $"CPU: {cpuPercent:0.0}% / 100%";
        //        RamLabel.Text = $"RAM: {ramUsedGb:0.0} GB / {ramTotalGb:0.0} GB";
        //    });
        //}
    }
}