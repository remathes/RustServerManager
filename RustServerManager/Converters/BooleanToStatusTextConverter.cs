using System;
using System.Globalization;
using System.Windows.Data;

namespace RustServerManager.Converters
{
    public class BooleanToStatusTextConverter : IValueConverter
    {
        public string ConnectedText { get; set; } = "Connected";
        public string DisconnectedText { get; set; } = "Not Connected";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? ConnectedText : DisconnectedText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}