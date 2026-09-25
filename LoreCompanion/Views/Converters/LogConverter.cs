using System.Globalization;
using System.Windows.Data;
using LoreCompanion.Utilities;
using Serilog;

namespace LoreCompanion.Views.Converters
{
    public class LogConverter : IValueConverter
    {
        private static ILogger Logger => field ??= LogManager.GetLogger();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Logger.Debug("Converting value '{Value}'", value);

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Logger.Debug("Converting value '{Value}' back", value);

            return value;
        }
    }
}