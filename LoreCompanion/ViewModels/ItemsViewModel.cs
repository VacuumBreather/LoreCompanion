using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public class ItemsViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailSectionScreen<Item>(
        NavigationSection.Lore,
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        public BindableCollection<Episode> Episodes { get; } = new();

        public IReadOnlyList<ItemType> ItemTypes { get; } = Enum.GetValues<ItemType>();

        protected override Item CreateEntityInstance()
        {
            return new Item { Name = "New Item" };
        }

        protected override bool FilterEntity(Item entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   entity.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        protected override IQueryable<Item> GetAllItemsQuery(LoreDbContext context)
        {
            return context.Items.Include(x => x.Episode);
        }

        protected override void ClearAdditional()
        {
            Episodes.Clear();
        }

        protected override async Task UpdateAdditionalAsync(LoreDbContext context, CancellationToken cancellationToken)
        {
            try
            {
                Episodes.IsNotifying = false;

                await foreach (var episode in context.Episodes.AsNoTracking()
                                                     .AsAsyncEnumerable()
                                                     .WithCancellation(cancellationToken))
                {
                    Episodes.Add(episode);
                }
            }
            finally
            {
                Episodes.Refresh();
                Episodes.IsNotifying = true;
            }
        }
    }
}