using System.Windows;
using Caliburn.Micro;
using LoreCompanion.Utilities;
using Microsoft.Xaml.Behaviors;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.Views.Behaviors
{
    public abstract class LoadCachedDataBehavior : Behavior<FrameworkElement>
    {
        private static CachedDataLoader? _cachedDataLoader;

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
            nameof(Url),
            typeof(string),
            typeof(LoadCachedDataBehavior),
            new PropertyMetadata(null, OnUrlChanged));

        private CancellationTokenSource? _cts;

        public string Url
        {
            get => (string)GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }

        protected static CachedDataLoader CachedDataLoader =>
            _cachedDataLoader ??= (CachedDataLoader)IoC.GetInstance(typeof(CachedDataLoader), null!);

        protected static ILogger Logger { get; } = LogManager.GetLogger();

        protected override void OnAttached()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;

            if (AssociatedObject.IsLoaded)
            {
                _ = UpdateTarget(Url);
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is not null)
            {
                AssociatedObject.Loaded -= OnLoaded;
                AssociatedObject.Unloaded -= OnUnloaded;
            }

            CancelInFlightRequest();
        }

        protected abstract Task UpdateTarget(string? dataUrl);

        protected CancellationToken ResetCancellationToken()
        {
            CancelInFlightRequest();
            _cts = new CancellationTokenSource();

            return _cts.Token;
        }

        protected bool IsCurrentCts(CancellationToken token)
        {
            return _cts is not null && (_cts.Token == token) && !token.IsCancellationRequested;
        }

        protected void CancelInFlightRequest()
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _ = UpdateTarget(Url);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CancelInFlightRequest();
        }

        private static async void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (d is not LoadCachedDataBehavior behavior || behavior.AssociatedObject is null)
                {
                    return;
                }

                await behavior.UpdateTarget(e.NewValue as string);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error updating cached data for {BehaviorType}", d.GetType().Name);
            }
        }
    }

    public abstract class LoadCachedDataBehavior<T> : LoadCachedDataBehavior
        where T : class
    {
        protected override async Task UpdateTarget(string? dataUrl)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                CancelInFlightRequest();
                SetDataProperty(AssociatedObject, null);

                return;
            }

            var token = ResetCancellationToken();

            try
            {
                var bytes = await CachedDataLoader.GetDataAsync(dataUrl, token);

                if (bytes is null)
                {
                    if (IsCurrentCts(token) && (Url == dataUrl))
                    {
                        SetDataProperty(AssociatedObject, null);
                    }

                    return;
                }

                var data = CreateInstanceFromBytes(bytes);

                if (IsCurrentCts(token) && (Url == dataUrl))
                {
                    SetDataProperty(AssociatedObject, data);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when a new URL is assigned before the current load finishes; ignore
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading data from {Url}", dataUrl);

                if (IsCurrentCts(token) && (Url == dataUrl))
                {
                    SetDataProperty(AssociatedObject, null);
                }
            }
        }

        protected abstract void SetDataProperty(DependencyObject element, T? source);

        protected abstract T? CreateInstanceFromBytes(byte[] data);
    }
}