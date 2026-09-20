using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LoreCompanion.Views.Helpers
{
    public static class ImageHelper
    {
        private static readonly HttpClient HttpClient = new();

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(ImageUrlProperty)),
            typeof(string),
            typeof(ImageHelper),
            new PropertyMetadata(null, OnImageUrlChanged));

        [AttachedPropertyBrowsableForType(typeof(Image))]
        [AttachedPropertyBrowsableForType(typeof(ImageBrush))]
        public static string? GetImageUrl(DependencyObject element)
        {
            return (string?)element.GetValue(ImageUrlProperty);
        }

        public static void SetImageUrl(DependencyObject element, string? value)
        {
            element.SetValue(ImageUrlProperty, value);
        }

        private static async void OnImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetUrl = e.NewValue as string;

            if (string.IsNullOrWhiteSpace(targetUrl) || !Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
            {
                SetImageSource(d, null);
                return;
            }

            try
            {
                var bitmap = await LoadImageAsync(uri);

                // Prevent race conditions: ensure the target URL has not changed during download
                if (GetImageUrl(d) == targetUrl)
                {
                    SetImageSource(d, bitmap);
                }
            }
            catch
            {
                // Clear source or retain placeholder on network/decoding failure
                if (GetImageUrl(d) == targetUrl)
                {
                    SetImageSource(d, null);
                }
            }
        }

        private static async Task<BitmapSource?> LoadImageAsync(Uri uri)
        {
            if (uri.Scheme is "http" or "https")
            {
                var bytes = await HttpClient.GetByteArrayAsync(uri);
                return CreateBitmapFromBytes(bytes);
            }

            // Fallback for local files and pack:// application resources
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        private static BitmapSource CreateBitmapFromBytes(byte[] data)
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

        private static void SetImageSource(DependencyObject element, ImageSource? source)
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
    }
}