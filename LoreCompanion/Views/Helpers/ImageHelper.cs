using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using LoreCompanion.Utilities;

namespace LoreCompanion.Views.Helpers
{
    public static class ImageHelper
    {
        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(ImageUrlProperty)),
            typeof(string),
            typeof(ImageHelper),
            new PropertyMetadata(null, OnImageUrlChanged));

        // Private attached property to track the in-flight CancellationTokenSource per DependencyObject
        private static readonly DependencyProperty CancellationTokenSourceProperty =
            DependencyProperty.RegisterAttached(
                DependencyPropertyNameHelper.GetName(nameof(CancellationTokenSourceProperty)),
                typeof(CancellationTokenSource),
                typeof(ImageHelper),
                new PropertyMetadata(null));

        [AttachedPropertyBrowsableForType(typeof(Image)), AttachedPropertyBrowsableForType(typeof(ImageBrush))]
        public static string? GetImageUrl(DependencyObject element)
        {
            return (string?)element.GetValue(ImageUrlProperty);
        }

        public static void SetImageUrl(DependencyObject element, string? value)
        {
            element.SetValue(ImageUrlProperty, value);
        }

        private static CancellationTokenSource? GetCancellationTokenSource(DependencyObject element)
        {
            return (CancellationTokenSource?)element.GetValue(CancellationTokenSourceProperty);
        }

        private static void SetCancellationTokenSource(DependencyObject element, CancellationTokenSource? value)
        {
            element.SetValue(CancellationTokenSourceProperty, value);
        }

        private static async void OnImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CancellationTokenSource? cts = null;
            string? targetUrl = null;

            try
            {
                var existingCts = GetCancellationTokenSource(d);

                if (existingCts is not null)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    // We want to wait synchronously here
                    existingCts.Cancel();
                    existingCts.Dispose();
                    SetCancellationTokenSource(d, null);
                }

                targetUrl = e.NewValue as string;

                if (string.IsNullOrWhiteSpace(targetUrl))
                {
                    SetImageSource(d, null);

                    return;
                }

                cts = new CancellationTokenSource();
                SetCancellationTokenSource(d, cts);
                var cachedDataLoader = (CachedDataLoader)IoC.GetInstance(typeof(CachedDataLoader), null!);
                var bytes = await cachedDataLoader.GetDataAsync(targetUrl, cts.Token);

                if (bytes == null)
                {
                    SetImageSource(d, null);

                    return;
                }

                var bitmap = bytes.CreateBitmapFromBytes();

                // Ensure the element has not been repurposed or changed during decoding
                if (!cts.IsCancellationRequested && (GetImageUrl(d) == targetUrl))
                {
                    SetImageSource(d, bitmap);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when a new URL is assigned before the current load finishes; ignore
            }
            catch
            {
                // Set source to null on genuine network or decoding failure if not canceled
                if (!(cts?.IsCancellationRequested ?? false) && (GetImageUrl(d) == targetUrl))
                {
                    SetImageSource(d, null);
                }
            }
            finally
            {
                // Clean up CTS reference if this task is still the active one
                if (GetCancellationTokenSource(d) == cts)
                {
                    SetCancellationTokenSource(d, null);
                    cts?.Dispose();
                }
            }
        }

        private static BitmapImage CreateBitmapFromBytes(this byte[] data)
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