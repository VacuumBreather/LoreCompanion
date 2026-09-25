using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LoreCompanion.Views.Converters
{
    public class IsNullToVisibilityConverter : IValueConverter
    {
        public Visibility NullVisibility { get; set; } = Visibility.Collapsed;

        public Visibility NotNullVisibility { get; set; } = Visibility.Visible;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is null ? NullVisibility : NotNullVisibility;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}