using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows.Media;

namespace RustServerManager.Converters
{
    public class BoolToBrushConverterProcess : IMultiValueConverter
    {
        public Brush TrueBrush { get; set; } = Brushes.Green;
        public Brush FalseBrush { get; set; } = Brushes.Red;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool isRunning)
                return isRunning ? TrueBrush : FalseBrush;
            return FalseBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}