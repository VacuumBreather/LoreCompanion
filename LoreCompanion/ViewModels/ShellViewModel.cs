using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Extensions;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public sealed class ShellViewModel : Conductor<SectionScreen>.Collection.OneActive
    {
        private readonly CachedDataLoader _cachedDataLoader;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        public ShellViewModel(
            IEnumerable<SectionScreen> sections,
            CachedDataLoader cachedDataLoader,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _cachedDataLoader = cachedDataLoader;
            _dialogService = dialogService;
            _notificationService = notificationService;

            ItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            ItemsView.GroupDescriptions!.Add(new PropertyGroupDescription(nameof(SectionScreen.Section)));

            ItemsView.CustomSort = Comparer<SectionScreen>.Create((a, b) =>
            {
                var result = NavigationSection.Order.IndexOf(a.Section)
                                              .CompareTo(NavigationSection.Order.IndexOf(b.Section));

                if (result != 0)
                {
                    return result;
                }

                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            Items.AddRange(sections);
        }

        public ListCollectionView ItemsView { get; }

        private static ILogger Logger { get; } = LogManager.GetLogger();

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new())
        {
            Logger.Information("Closing application...");

            await using var scope = await _dialogService.ShowBusyDialogAsync(
                                        "Please Wait",
                                        "Closing application...",
                                        cancellationToken);

            await _cachedDataLoader.DisposeAsync();

            if (_notificationService is IDeactivate deactivateNotifications)
            {
                await deactivateNotifications.DeactivateAsync(true, cancellationToken);
            }

            if (_dialogService is IDeactivate deactivateDialogs)
            {
                await deactivateDialogs.DeactivateAsync(true, cancellationToken);
            }

            return await base.CanCloseAsync(cancellationToken);
        }

        protected override async Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Application initialized");
            Logger.Information("Showing dashboard...");

            if (_notificationService is IActivate activateNotifications)
            {
                await activateNotifications.ActivateAsync(cancellationToken);
            }

            if (_dialogService is IActivate activateDialogs)
            {
                await activateDialogs.ActivateAsync(cancellationToken);
            }

            var firstGroup = (CollectionViewGroup)ItemsView.Groups!.First();
            var firstScreen = (SectionScreen)firstGroup.Items.First();

            await ActivateItemAsync(firstScreen, cancellationToken);
        }
    }
}