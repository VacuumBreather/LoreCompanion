using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public sealed class ItemsViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeLocationSectionScreen<Item>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override Item CreateEntityInstance()
        {
            return new Item { Name = "New Item" };
        }
    }
}