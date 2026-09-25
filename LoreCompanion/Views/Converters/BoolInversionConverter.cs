using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LoreCompanion.Views.Converters
{
    public class BoolInversionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
            {
                return DependencyProperty.UnsetValue;
            }

            return !boolValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}