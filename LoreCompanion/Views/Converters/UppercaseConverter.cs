using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoreCompanion.Views.Converters
{
    public class UppercaseConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value?.ToString()?.ToUpper(culture) ?? string.Empty;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static readonly UppercaseConverter Instance = new();

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}