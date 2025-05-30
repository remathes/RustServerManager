﻿using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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

