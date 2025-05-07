using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows.Media;

namespace RustServerManager.Converters

{
    public class BooleanToBrushConverter : IValueConverter
    {
        public Brush TrueBrush  { get; set; } = Brushes.LimeGreen;
        public Brush FalseBrush { get; set; } = Brushes.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}