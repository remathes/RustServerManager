using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RustServerManager.Converters
{
    public class ConnectionBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool isConnected)
            {
                return isConnected ? Brushes.LimeGreen : Brushes.Crimson;
            }
            return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
