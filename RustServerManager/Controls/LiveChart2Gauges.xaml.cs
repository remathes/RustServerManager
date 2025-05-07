using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RustServerManager.Controls
{
    /// <summary>
    /// Interaction logic for LiveChart2Gauges.xaml
    /// </summary>
    public partial class LiveChart2Gauges : UserControl
    {
        ViewModel vm = new ViewModel();
        public LiveChart2Gauges()
        {
            InitializeComponent();
            this.DataContext = vm;
            Gauges.Series = vm.Series;
        }

        public class ViewModel
        {
            public IEnumerable<ISeries> Series { get; set; } =
                GaugeGenerator.BuildSolidGauge(
                    new GaugeItem(30, series =>
                    {
                        series.Fill = new SolidColorPaint(SKColors.YellowGreen);
                        series.DataLabelsSize = 50;
                        series.DataLabelsPaint = new SolidColorPaint(SKColors.Red);
                        series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                        series.InnerRadius = 75;
                    }),
                    new GaugeItem(GaugeItem.Background, series =>
                    {
                        series.InnerRadius = 75;
                        series.Fill = new SolidColorPaint(new SKColor(100, 181, 246, 90));
                    }));
        }
    }
}
