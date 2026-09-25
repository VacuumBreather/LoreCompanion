using System.Globalization;
using System.Windows.Data;

namespace LoreCompanion.Views.Converters
{
    public class IsEqualConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return AreEqual(value, parameter);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static bool AreEqual<T>(T first, T second)
        {
            return EqualityComparer<T>.Default.Equals(first, second);
        }
    }
}