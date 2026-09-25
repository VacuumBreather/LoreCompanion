using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoreCompanion.Views.Converters
{
    public class QuoteConverter : MarkupExtension, IValueConverter
    {
        private static readonly QuoteConverter Instance = new();

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string str)

            {
                return DependencyProperty.UnsetValue;
            }

            return $"\"{str}\"";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}