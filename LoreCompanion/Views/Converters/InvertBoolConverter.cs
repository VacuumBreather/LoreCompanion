using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoreCompanion.Views.Converters
{
    public class InvertBoolConverter : MarkupExtension, IValueConverter
    {
        private static readonly InvertBoolConverter Instance = new();

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : DependencyProperty.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}