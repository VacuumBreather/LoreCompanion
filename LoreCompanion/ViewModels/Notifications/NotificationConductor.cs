using Caliburn.Micro;

namespace LoreCompanion.ViewModels.Notifications
{
    /// <summary>A conductor handling notifications.</summary>
    public sealed class NotificationConductor : Conductor<NotificationScreen>.Collection.AllActive, INotificationService
    {
        private static readonly TimeSpan MinimumExpirationTime = TimeSpan.FromSeconds(value: 1);

        /// <inheritdoc/>
        public TimeSpan ExpirationTime
        {
            get;
            set => Set(ref field, value < MinimumExpirationTime ? MinimumExpirationTime : value);
        } = TimeSpan.FromSeconds(value: 5);

        /// <inheritdoc/>
        public Task ShowNotificationAsync(
            string title,
            string content,
            NotificationType type = NotificationType.Information,
            TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default)
        {
            var notification = new NotificationScreen(title, content, type, expirationTime ?? ExpirationTime);

            return ActivateItemAsync(notification, cancellationToken);
        }
    }
}