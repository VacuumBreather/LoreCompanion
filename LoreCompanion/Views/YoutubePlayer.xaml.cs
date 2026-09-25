using System.Windows;
using Caliburn.Micro;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.Web.WebView2.Core;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.Views
{
    public partial class YoutubePlayer
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

        private static ILogger Logger => field ??= LogManager.GetLogger();

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
                    AttachCoreEvents();
                    PlayCurrentVideo();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize or play video");
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopVideo();
            DetachCoreEvents();
        }

        private void OnProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            Logger.Warning("WebView2 process failed: {Kind}, {Reason}", e.ProcessFailedKind, e.Reason);

            _isInitialized = false;
            WebView.CoreWebView2?.Navigate("about:blank");

            _ = IoC.Get<INotificationService>()
                   .ShowNotificationAsync(
                       "Youtube player",
                       $"Youtube player WebView2 process failed:\n{e.ProcessFailedKind}\n{e.Reason}",
                       NotificationType.Warning);
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

            var host = url.Host;

            return host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   host.Equals("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase) ||
                   host.EndsWith(".youtube-nocookie.com", StringComparison.OrdinalIgnoreCase);
        }

        private static void OnVideoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is YoutubePlayer { _isInitialized: true } player)
            {
                player.PlayCurrentVideo();
            }
        }

        private void AttachCoreEvents()
        {
            if (WebView.CoreWebView2 is not { } core)
            {
                return;
            }

            core.ProcessFailed -= OnProcessFailed;
            core.ProcessFailed += OnProcessFailed;

            core.WebResourceRequested -= OnWebResourceRequested;
            core.WebResourceRequested += OnWebResourceRequested;
        }

        private void DetachCoreEvents()
        {
            if (WebView.CoreWebView2 is not { } core)
            {
                return;
            }

            core.ProcessFailed -= OnProcessFailed;
            core.WebResourceRequested -= OnWebResourceRequested;
        }

        private void PlayCurrentVideo()
        {
            if (WebView.CoreWebView2 is null || string.IsNullOrWhiteSpace(VideoKey))
            {
                return;
            }

            var url = YouTubeHelper.BuildEmbedUrl(VideoKey, Timestamp);
            WebView.CoreWebView2.Navigate(url);
        }

        private void StopVideo()
        {
            WebView.CoreWebView2?.Navigate("about:blank");
        }

        private async Task InitializeAsync()
        {
            var options = new CoreWebView2EnvironmentOptions
            {
                AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required",
            };

            var userDataFolder = AppHelper.WebViewUserDataFolder;

            var environment = await CoreWebView2Environment.CreateAsync(
                                  userDataFolder: userDataFolder,
                                  options: options);

            await WebView.EnsureCoreWebView2Async(environment);

            var core = WebView.CoreWebView2;

            core.AddWebResourceRequestedFilter("https://www.youtube.com/*", CoreWebView2WebResourceContext.All);
            core.AddWebResourceRequestedFilter("https://youtube.com/*", CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter(
                "https://www.youtube-nocookie.com/*",
                CoreWebView2WebResourceContext.All);

            core.AddWebResourceRequestedFilter("https://youtube-nocookie.com/*", CoreWebView2WebResourceContext.All);
            core.AddWebResourceRequestedFilter("https://*.youtube.com/*", CoreWebView2WebResourceContext.All);
            core.AddWebResourceRequestedFilter("https://*.youtube-nocookie.com/*", CoreWebView2WebResourceContext.All);

            AttachCoreEvents();

            _isInitialized = true;
            PlayCurrentVideo();
        }
    }
}