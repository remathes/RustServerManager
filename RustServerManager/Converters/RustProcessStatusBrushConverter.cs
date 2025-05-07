using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace RustServerManager.Converters
{
    public class RustProcessStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string installPath)
            {
                try
                {
                    string exePath = Path.Combine(installPath, "RustDedicated.exe");

                    return Process.GetProcessesByName("RustDedicated")
                        .Any(p =>
                        {
                            try
                            {
                                return string.Equals(p.MainModule?.FileName, exePath, StringComparison.OrdinalIgnoreCase);
                            }
                            catch { return false; }
                        })
                        ? Brushes.LimeGreen
                        : Brushes.Crimson;
                }
                catch { }
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
