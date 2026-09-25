using Caliburn.Micro;

namespace LoreCompanion.ViewModels.Notifications
{
    /// <summary>Represents a notification displayed to inform the user.</summary>
    public class NotificationScreen : Screen
    {
        private static readonly TimeSpan MinimumExpirationTime = TimeSpan.FromSeconds(value: 1);
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>Initializes a new instance of the <see cref="NotificationScreen"/> class.</summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="content">The content of the notification.</param>
        /// <param name="type">The type of the notification.</param>
        /// <param name="expirationTime">
        ///     (Optional) The expiration time after which notifications are automatically closed. The
        ///     minimum is one second. If this is not provided, the global expiration time set on the
        ///     <see cref="INotificationService"/> will be used.
        /// </param>
        public NotificationScreen(
            string title,
            string content,
            NotificationType type = NotificationType.Information,
            TimeSpan? expirationTime = null)
        {
            Title = title;
            Content = content;
            Type = type;

            if (expirationTime is not null)
            {
                expirationTime = expirationTime.Value < MinimumExpirationTime
                                     ? MinimumExpirationTime
                                     : expirationTime.Value;
            }

            ExpirationTime = expirationTime;
        }

        /// <summary>Gets the content of the notification.</summary>
        public string Content { get; }

        /// <summary>
        ///     Gets the expiration time after which notifications are automatically closed. The minimum is one second. If
        ///     this is <see cref="TimeSpan.Zero"/>, the global expiration time set on the <see cref="INotificationService"/> will
        ///     be used.
        /// </summary>
        public TimeSpan? ExpirationTime { get; }

        /// <summary>Gets the title of the dialog.</summary>
        public string Title { get; }

        /// <summary>Gets the type of the notification.</summary>
        public NotificationType Type { get; }

        public void Close()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <inheritdoc/>
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            if (Parent is not NotificationConductor notificationConductor)
            {
                throw new InvalidOperationException(
                    $"A {nameof(NotificationScreen)} must be conducted by a {nameof(NotificationConductor)}.");
            }

            var usedExpirationTime = ExpirationTime ?? notificationConductor.ExpirationTime;
            _cancellationTokenSource = new CancellationTokenSource();

            _ = Execute.OnUIThreadAsync(async () =>
            {
                try
                {
                    await Task.Delay(usedExpirationTime, _cancellationTokenSource.Token);
                }
                finally
                {
                    await TryCloseAsync();
                }
            });

            return Task.CompletedTask;
        }
    }
}