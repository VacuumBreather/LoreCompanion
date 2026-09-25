using System.Windows;
using System.Windows.Controls;
using LoreCompanion.Utilities;
using Microsoft.Web.WebView2.Core;
using Serilog;

namespace LoreCompanion.Views
{
    public partial class YoutubePlayer : UserControl
    {
        public static readonly DependencyProperty VideoKeyProperty = DependencyProperty.Register(
            nameof(VideoKey),
            typeof(string),
            typeof(YoutubePlayer),
            new PropertyMetadata(null, OnVideoSourceChanged));

        public static readonly DependencyProperty TimestampProperty = DependencyProperty.Register(
            nameof(Timestamp),
            typeof(TimeSpan),
            typeof(YoutubePlayer),
            new PropertyMetadata(TimeSpan.Zero, OnVideoSourceChanged));

        private bool _isInitialized;

        public YoutubePlayer()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public string? VideoKey
        {
            get => (string?)GetValue(VideoKeyProperty);
            set => SetValue(VideoKeyProperty, value);
        }

        public TimeSpan Timestamp
        {
            get => (TimeSpan)GetValue(TimestampProperty);
            set => SetValue(TimestampProperty, value);
        }

        public void PlayCurrentVideo()
        {
            if (WebView.CoreWebView2 is null || string.IsNullOrWhiteSpace(VideoKey))
            {
                return;
            }

            var url = YouTubeHelper.BuildEmbedUrl(VideoKey, Timestamp);
            WebView.CoreWebView2.Navigate(url);
        }

        public void StopVideo()
        {
            if (WebView.CoreWebView2 is not null)
            {
                WebView.CoreWebView2.Navigate("about:blank");
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }
                else
                {
                    PlayCurrentVideo();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize or play video");
            }
        }

        private static ILogger Logger => field ??= LogManager.GetLogger();

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }

        private void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (!IsYoutubeRequest(e.Request.Uri))
            {
                return;
            }

            e.Request.Headers.SetHeader("Referer", YouTubeHelper.Referer);
        }

        private static bool IsYoutubeRequest(string uri)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var url))
            {
                return false;
            }

            return url.Host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Host.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Host.Equals("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase) ||
                   url.Host.Equals("www.youtube-nocookie.com", StringComparison.OrdinalIgnoreCase);
        }

        private static void OnVideoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is YoutubePlayer { _isInitialized: true } player)
            {
                player.PlayCurrentVideo();
            }
        }

        private async Task InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async();

            var core = WebView.CoreWebView2;

            core.AddWebResourceRequestedFilter("https://www.youtube.com/*", CoreWebView2WebResourceContext.All);
            core.AddWebResourceRequestedFilter("https://youtube.com/*", CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter(
                "https://www.youtube-nocookie.com/*",
                CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter("https://youtube-nocookie.com/*", CoreWebView2WebResourceContext.All);

            core.WebResourceRequested += OnWebResourceRequested;

            _isInitialized = true;
            PlayCurrentVideo();
        }
    }
}