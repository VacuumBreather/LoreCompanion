using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace LoreCompanion.Views
{
    public partial class YoutubePlayer : UserControl
    {
        // This identifies your application to YouTube.
        //
        // Use an HTTPS URL identifying your application.
        private const string Referer = "https://vacuumbreather.de/lorecompanion/";

        private bool _initialized;

        public YoutubePlayer()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_initialized) return;

            _initialized = true;

            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async();

            var core = WebView.CoreWebView2;

            // IMPORTANT:
            //
            // The Referer has to be added to requests made by the
            // embedded player as well, not just the initial navigation.
            core.AddWebResourceRequestedFilter("https://www.youtube.com/*", CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter("https://youtube.com/*", CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter(
                "https://www.youtube-nocookie.com/*",
                CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter("https://youtube-nocookie.com/*", CoreWebView2WebResourceContext.All);

            core.WebResourceRequested += OnWebResourceRequested;

            LoadVideo("BhFtxDrIoYk");
        }

        private void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var request = e.Request;

            if (!IsYoutubeRequest(request.Uri)) return;

            request.Headers.SetHeader("Referer", Referer);
        }

        private static bool IsYoutubeRequest(string uri)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var url)) return false;

            return url.Host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Host.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Host.Equals("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Host.Equals("www.youtube-nocookie.com", StringComparison.OrdinalIgnoreCase);
        }

        public void LoadVideo(string videoId)
        {
            if (WebView.CoreWebView2 is null)
                throw new InvalidOperationException("YouTubePlayer has not been initialized.");

            if (string.IsNullOrWhiteSpace(videoId))
                throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));

            var url = "https://www.youtube.com/embed/" +
                      Uri.EscapeDataString(videoId) +
                      "?autoplay=1" +
                      "?enablejsapi=1" +
                      "&origin=" +
                      Uri.EscapeDataString(Referer.TrimEnd('/'));

            WebView.CoreWebView2.Navigate(url);
        }
    }
}