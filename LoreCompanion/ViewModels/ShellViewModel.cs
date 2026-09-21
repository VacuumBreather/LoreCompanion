using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Extensions;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public sealed class ShellViewModel : Conductor<SectionScreen>.Collection.OneActive
    {
        private readonly CachedDataLoader _cachedDataLoader;
        private readonly IDialogService _dialogService;

        public ShellViewModel(
            IEnumerable<SectionScreen> sections,
            CachedDataLoader cachedDataLoader,
            IDialogService dialogService)
        {
            _cachedDataLoader = cachedDataLoader;
            _dialogService = dialogService;
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

            return await base.CanCloseAsync(cancellationToken);
        }

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Application initialized");
            Logger.Information("Showing dashboard...");

            var firstGroup = (CollectionViewGroup)ItemsView.Groups!.First();
            var firstScreen = (SectionScreen)firstGroup.Items.First();

            return ActivateItemAsync(firstScreen, cancellationToken);
        }
    }
}