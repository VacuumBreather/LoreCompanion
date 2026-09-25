using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using LoreCompanion.Views.Helpers;
using MahApps.Metro.IconPacks;

namespace LoreCompanion.Views.Converters
{
    [ContentProperty(nameof(Mappings))]
    public class NavigationIconConverter : IValueConverter
    {
        [SuppressMessage(
            "ReSharper",
            "CollectionNeverUpdated.Global",
            Justification = "Converters collection is populated by XAML")]
        public Collection<TypeIconMapping> Mappings { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return PackIconBoxIconsKind.None;
            }

            var type = value.GetType();

            var mapping = Mappings.FirstOrDefault(x => x.Type == type);

            return mapping?.Icon ?? PackIconBoxIconsKind.None;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}