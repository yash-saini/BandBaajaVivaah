using System.Globalization;
using System.Windows.Data;

namespace BandBaajaVivaah.WPF.Converters
{
    public class RadioButtonToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Compares the ViewModel's string value to this radio button's specific value (from the parameter)
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If this radio button is checked (value is true), return its value to the ViewModel
            if (value is true)
                return parameter;

            return Binding.DoNothing;
        }
    }
}
