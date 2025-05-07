using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RustServerManager.Converters
{
    public class BannerImageConverter : IValueConverter
    {
        private static readonly BitmapImage DefaultImage = new BitmapImage(new Uri("pack://application:,,,/Assets/RustLogo.png"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    if (File.Exists(path))
                    {
                        return new BitmapImage(new Uri(path, UriKind.Absolute));
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return DefaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}