using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MaterialDesignThemes.Wpf;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RustServerManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        LiveCharts.Configure(config => config
        .AddSkiaSharp() 
        .AddDefaultMappers());
        base.OnStartup(e);
    }
}

