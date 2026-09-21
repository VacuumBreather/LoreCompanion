using System.Runtime.CompilerServices;
using Caliburn.Micro;

namespace LoreCompanion.ViewModels.Dialogs
{
    /// <summary>A conductor handling dialogs.</summary>
    public sealed class DialogConductor : Conductor<DialogScreen>.Collection.OneActive, IDialogService
    {
        private readonly QueryDialog _queryDialog = new("", "", DialogResults.Ok, DialogResult.Ok);

        private readonly Dictionary<DialogScreen, TaskCompletionSource<DialogResult>> _activeTrackers =
            new(new IdentityComparer<DialogScreen>());

        /// <summary>Initializes a new instance of the <see cref="DialogConductor"/> class.</summary>
        public DialogConductor()
        {
            DisplayName = GetType().Name;
        }

        public override async Task DeactivateItemAsync(
            DialogScreen item,
            bool close,
            CancellationToken cancellationToken = default)
        {
            await base.DeactivateItemAsync(item, close, cancellationToken);

            if (close)
            {
                if (_activeTrackers.Remove(item, out var tcs))
                {
                    tcs.TrySetResult(item.DialogResult);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DialogResult> ShowDialogAsync(
            DialogScreen dialog,
            CancellationToken cancellationToken = default)
        {
            if (Items.Contains(dialog))
            {
                throw new ArgumentException(
                    $"Attempting to open a {dialog.GetType().Name} dialog with the same instance multiple times simultaneously.",
                    nameof(dialog));
            }

            var tcs = new TaskCompletionSource<DialogResult>();
            _activeTrackers[dialog] = tcs;

            try
            {
                await ActivateItemAsync(dialog, cancellationToken);

                return await tcs.Task;
            }
            catch
            {
                _activeTrackers.Remove(dialog);

                throw;
            }
        }

        /// <inheritdoc/>
        public Task<DialogResult> ShowQueryDialogAsync(
            string title,
            string content,
            DialogResults dialogResults,
            DialogResult defaultResult = DialogResult.None,
            CancellationToken cancellationToken = default)
        {
            _queryDialog.Title = title;
            _queryDialog.Content = content;
            _queryDialog.DialogResults = dialogResults;
            _queryDialog.DefaultResult = defaultResult;

            return ShowDialogAsync(_queryDialog, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<DialogResult> ShowInformationDialogAsync(
            string title,
            string content,
            CancellationToken cancellationToken = default)
        {
            _queryDialog.Title = title;
            _queryDialog.Content = content;
            _queryDialog.DialogResults = DialogResults.Ok;
            _queryDialog.DefaultResult = DialogResult.Ok;

            return ShowDialogAsync(_queryDialog, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> ShowBusyDialogAsync(
            string title,
            string content,
            CancellationToken cancellationToken = default)
        {
            var busyDialog = new QueryDialog(title, content, DialogResults.None);
            await ActivateItemAsync(busyDialog, cancellationToken);

            return new BusyDialogScope(busyDialog);
        }

        private sealed class BusyDialogScope(DialogScreen dialog) : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                await dialog.CloseDialogAsync(DialogResult.None);
            }
        }

        private sealed class IdentityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            public bool Equals(T? x, T? y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}