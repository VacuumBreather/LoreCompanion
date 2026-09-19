using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Extensions;

namespace LoreCompanion.ViewModels
{
    public class ShellViewModel : Conductor<SectionScreen>.Collection.OneActive
    {
        public  ListCollectionView ItemsView { get; }

        public ShellViewModel(IEnumerable<SectionScreen> sections)
        {
            ItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            ItemsView.GroupDescriptions!.Add(new PropertyGroupDescription(nameof(SectionScreen.Section)));

            ItemsView.CustomSort = Comparer<SectionScreen>.Create((a, b) =>
            {
                var result = NavigationSection.Order.IndexOf(a.Section)
                                              .CompareTo(
                                                  NavigationSection.Order
                                                                   .IndexOf(b.Section));

                if (result != 0) return result;

                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            Items.AddRange(sections);
        }

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return ActivateItemAsync(Items.First(), cancellationToken);
        }
    }
}