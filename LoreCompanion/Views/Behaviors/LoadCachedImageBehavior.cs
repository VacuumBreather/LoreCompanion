using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LoreCompanion.Views.Behaviors
{
    public class LoadCachedImageBehavior : LoadCachedDataBehavior<ImageSource>
    {
        protected override void SetDataProperty(DependencyObject element, ImageSource? source)
        {
            if (element is Image image)
            {
                image.Source = source;
            }
            else if (element is ImageBrush imageBrush)
            {
                imageBrush.ImageSource = source;
            }
        }

        protected override BitmapImage? CreateInstanceFromBytes(byte[] data)
        {
            try
            {
                var bitmap = new BitmapImage();

                using (var stream = new MemoryStream(data))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                bitmap.Freeze(); // Crucial for cross-thread rendering & memory efficiency

                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create bitmap from cached bytes");

                return null;
            }
        }
    }
}