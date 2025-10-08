using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BandBaajaVivaah.WPF.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If the value is true, we want to HIDE the control (Collapsed).
                // If the value is false, we want to SHOW the control (Visible).
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            // Default to collapsed if the value is not a boolean.
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
