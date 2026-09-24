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
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeLocationSectionScreen<Item>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override int CompareEntities(Item x, Item y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        protected override Item CreateEntityInstance()
        {
            return new Item { Name = "New Item" };
        }

        protected override bool FilterEntity(Item entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   entity.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}