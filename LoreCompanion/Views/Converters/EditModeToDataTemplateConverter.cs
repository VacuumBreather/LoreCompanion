using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LoreCompanion.ViewModels;

namespace LoreCompanion.Views.Converters
{
    public class EditModeToDataTemplateConverter : IValueConverter
    {
        public DataTemplate? ReadOnlyTemplate { get; set; }

        public DataTemplate? EditTemplate { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not EditMode editMode) return DependencyProperty.UnsetValue;

            return editMode == EditMode.ReadOnly ? ReadOnlyTemplate : EditTemplate;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}