using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LoreCompanion.Views.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueVisibility { get; set; } = Visibility.Visible;

        public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
            {
                return DependencyProperty.UnsetValue;
            }

            return boolValue ? TrueVisibility : FalseVisibility;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}