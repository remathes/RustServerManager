using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;
using System.IO;
using System.Linq;

namespace RustServerManager.Converters
{
    public class EditableFileVisibilityConverter : IValueConverter
    {
        private static readonly string[] editableExtensions = { ".txt", ".log", ".cfg", ".xml", ".json", ".csv" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileName)
            {
                var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                return editableExtensions.Contains(ext) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}