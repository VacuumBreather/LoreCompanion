using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Extensions;
using LoreCompanion.Utilities;

namespace LoreCompanion.ViewModels
{
    public sealed class ShellViewModel : Conductor<SectionScreen>.Collection.OneActive
    {
        private readonly CachedDataLoader _cachedDataLoader;

        public ShellViewModel(IEnumerable<SectionScreen> sections, CachedDataLoader cachedDataLoader)
        {
            _cachedDataLoader = cachedDataLoader;
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

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            var firstGroup = (CollectionViewGroup)ItemsView.Groups!.First();
            var firstScreen = (SectionScreen)firstGroup.Items.First();

            return ActivateItemAsync(firstScreen, cancellationToken);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new())
        {
            await _cachedDataLoader.DisposeAsync();

            return await base.CanCloseAsync(cancellationToken);
        }
    }
}