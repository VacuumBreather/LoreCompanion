using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LoreCompanion.Views.Converters
{
    public class BooleanAndToVisibilityMultiConverter : IMultiValueConverter
    {
        public Visibility TrueVisibility { get; set; } = Visibility.Visible;

        public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(value => value is true) ? TrueVisibility : FalseVisibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}