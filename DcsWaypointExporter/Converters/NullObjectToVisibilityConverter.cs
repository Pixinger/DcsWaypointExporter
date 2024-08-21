// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DcsWaypointExporter.Converters
{
    /// <summary>
    /// Konvertiert einen Wert <see langword="null"/> in <see cref="Visibility.Collapsed"/> und not <see langword="null"/> in <see cref="Visibility.Visible"/>. Mittels Parameter kann der Konverter invertiert werden. Bei <see langword="true"/> erfolgt die Konvertierung zu <see cref="Visibility.Visible"/> im Falle von <see langword="null"/> und umgekehrt.
    /// </summary>
    public class NullObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((parameter is null) || (parameter.GetType() != typeof(string)))
            {
                return value is not null ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                var text = ((string)parameter).ToLower();
                if (!text.Equals("true"))
                {
                    return value is null ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return value is null ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
