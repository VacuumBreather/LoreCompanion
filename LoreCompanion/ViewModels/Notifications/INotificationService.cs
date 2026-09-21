using Caliburn.Micro;

namespace LoreCompanion.ViewModels.Notifications
{
    /// <summary>Interface for a service handling notifications.</summary>
    public interface INotificationService : IConductor, IParent<NotificationScreen>
    {
        /// <summary>Gets the items that are currently being conducted.</summary>
        IObservableCollection<NotificationScreen> Items { get; }

        /// <summary>
        ///     Gets or sets the expiration time after which notifications are automatically closed. The minimum is one
        ///     second.
        /// </summary>
        TimeSpan ExpirationTime { get; set; }

        /// <summary>Shows the specified <see cref="NotificationScreen"/> as a notification.</summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="content">The content of the notification.</param>
        /// <param name="type">The type of the notification.</param>
        /// <param name="expirationTime">
        ///     (Optional) The expiration time after which notifications are automatically closed. The
        ///     minimum is one second. If this is not provided, the global expiration time set on the
        ///     <see cref="INotificationService"/> will be used.
        /// </param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects or threads to receive
        ///     notice of cancellation.
        /// </param>
        /// <returns>A Task that represents the asynchronous save operation.</returns>
        Task ShowNotificationAsync(
            string title,
            string content,
            NotificationType type = NotificationType.Information,
            TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default);
    }
}