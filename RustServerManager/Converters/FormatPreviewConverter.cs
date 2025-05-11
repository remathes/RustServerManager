using System;
using System.Globalization;
using System.Windows.Data;

namespace RustServerManager.Converters
{
    public class FormatPreviewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string template = values[0]?.ToString() ?? string.Empty;
            string seconds = values[1]?.ToString() ?? "30";

            return $"Preview: {template.Replace("{seconds}", seconds)}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}