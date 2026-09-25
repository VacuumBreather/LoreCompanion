using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using LoreCompanion.Dtos;

namespace LoreCompanion.Views.Behaviors
{
    public class LoadCachedVideoTitleBehavior : LoadCachedDataBehavior<YouTubeMetaData>
    {
        protected override void SetDataProperty(DependencyObject element, YouTubeMetaData? source)
        {
            if (element is TextBlock textBlock)
            {
                textBlock.SetCurrentValue(TextBlock.TextProperty, source?.Title);
            }
        }

        protected override YouTubeMetaData? CreateInstanceFromBytes(byte[] data)
        {
            try
            {
                return JsonSerializer.Deserialize<YouTubeMetaData>(data);
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "Failed to deserialize {DtoName}", nameof(YouTubeMetaData));

                return null;
            }
        }
    }
}