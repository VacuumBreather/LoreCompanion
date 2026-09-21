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
        } = TimeSpan.FromSeconds(value: 10);

        /// <inheritdoc/>
        public Task ShowNotificationAsync(
            NotificationScreen notification,
            CancellationToken cancellationToken = default)
        {
            if (Items.Contains(notification))
            {
                throw new InvalidOperationException(
                    $"Attempting to show a {notification.GetType().Name} notification with the same instance multiple times simultaneously.");
            }

            return ActivateItemAsync(notification, cancellationToken);
        }
    }
}