using Caliburn.Micro;

namespace LoreCompanion.ViewModels.Dialogs
{
    /// <summary>A base class for dialog screens.</summary>
    public abstract class DialogScreen : Screen
    {
        /// <inheritdoc/>
        public sealed override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public sealed override Task TryCloseAsync(bool? dialogResult = null)
        {
            return base.TryCloseAsync(dialogResult);
        }

        /// <summary>Closes the dialog with the given result.</summary>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task CloseDialogAsync(DialogResult dialogResult)
        {
            DialogResult = dialogResult;

            return TryCloseAsync();
        }

        /// <summary>Called when the dialog is opened.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual Task OnDialogOpenedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>Called when the dialog is being closed.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual Task OnDialogClosed(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected sealed override async Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            DialogResult = DialogResult.None;

            await OnDialogOpenedAsync(cancellationToken);
            await base.OnActivatedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected sealed override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (IsInitialized && close)
            {
                await OnDialogClosed(cancellationToken);
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        /// <summary>Gets the dialog result.</summary>
        internal DialogResult DialogResult { get; private set; } = DialogResult.None;
    }
}