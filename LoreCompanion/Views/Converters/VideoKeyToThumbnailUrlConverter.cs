using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LoreCompanion.Utilities;

namespace LoreCompanion.Views.Converters
{
    public class VideoKeyToThumbnailUrlConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string videoKey || string.IsNullOrWhiteSpace(videoKey))
            {
                return DependencyProperty.UnsetValue;
            }

            return string.Format(YouTubeHelper.ThumbnailUrlFormatString, videoKey);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}