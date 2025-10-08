using System.Collections;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BandBaajaVivaah.WPF.Converters
{
    public class ValidationErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var errors = value as IEnumerable;
            if (errors != null)
            {
                var firstError = errors.OfType<ValidationError>().FirstOrDefault();
                return firstError?.ErrorContent;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
