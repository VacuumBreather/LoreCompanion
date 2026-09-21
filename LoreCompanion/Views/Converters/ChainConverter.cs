using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoreCompanion.Views.Converters
{
    [ContentProperty(nameof(Converters))]
    public sealed class ChainConverter : IValueConverter
    {
        [SuppressMessage(
            "ReSharper",
            "CollectionNeverUpdated.Global",
            Justification = "Converters collection is populated by XAML")]
        public Collection<IValueConverter> Converters { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            foreach (var converter in Converters)
            {
                value = converter.Convert(value, targetType, parameter, culture);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            for (var i = Converters.Count - 1; i >= 0; i--)
            {
                value = Converters[i].ConvertBack(value, targetType, parameter, culture);
            }

            return value;
        }
    }
}