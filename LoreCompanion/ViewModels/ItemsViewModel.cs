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
        protected override Item CreateEntityInstance()
        {
            return new Item { Name = "New Item", Description = "Item Description" };
        }

        protected override bool FilterEntity(Item entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   entity.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}