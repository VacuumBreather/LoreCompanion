using Caliburn.Micro;

namespace LoreCompanion.ViewModels.Dialogs
{
    /// <summary>Interface for a service for opening various dialogs and awaiting their results.</summary>
    public interface IDialogService : IScreen, IConductActiveItem
    {
        /// <summary>Shows the specified <see cref="DialogScreen"/> as a dialog.</summary>
        /// <param name="dialog">The dialog to show.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects or threads to receive
        ///     notice of cancellation.
        /// </param>
        /// <returns>A Task that represents the asynchronous save operation. The Task result contains the dialog result.</returns>
        Task<DialogResult> ShowDialogAsync(DialogScreen dialog, CancellationToken cancellationToken = default);

        /// <summary>Shows the specified <see cref="DialogScreen"/> as a dialog.</summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="content">The content text of the dialog.</param>
        /// <param name="dialogResults">The possible results the dialog can return.</param>
        /// <param name="defaultResult">(Optional) The default result.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects or threads to receive
        ///     notice of cancellation.
        /// </param>
        /// <returns>A Task that represents the asynchronous save operation. The Task result contains the dialog result.</returns>
        Task<DialogResult> ShowQueryDialogAsync(
            string title,
            string content,
            DialogResults dialogResults,
            DialogResult defaultResult = DialogResult.None,
            CancellationToken cancellationToken = default);

        /// <summary>Shows the specified <see cref="DialogScreen"/> as a dialog.</summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="content">The content text of the dialog.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects or threads to receive
        ///     notice of cancellation.
        /// </param>
        /// <returns>A Task that represents the asynchronous save operation. The Task result contains the dialog result.</returns>
        Task<DialogResult> ShowInformationDialogAsync(
            string title,
            string content,
            CancellationToken cancellationToken = default);

        /// <summary>Displays a busy dialog with the specified title and content.</summary>
        /// <param name="title">The title of the busy dialog.</param>
        /// <param name="content">The content or description to be shown in the busy dialog.</param>
        /// <param name="cancellationToken">
        /// (Optional) A cancellation token that can be used to receive notice of cancellation.
        /// </param>
        /// <returns>A Task that represents the asynchronous operation. The Task result contains a disposable scope for managing the dialog lifecycle.</returns>
        Task<IAsyncDisposable> ShowBusyDialogAsync(
            string title,
            string content,
            CancellationToken cancellationToken = default);
    }
}