using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public class LocationsViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeSectionScreen<Location>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override Location CreateEntityInstance()
        {
            return new Location { Name = "New Location" };
        }

        protected override bool FilterEntity(Location entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<bool> CanDeleteAsync(LoreDbContext context, Location entity)
        {
            try
            {
                var usedInItems =
                    await context.Items.AnyAsync(i => (i.LocationId != null) && (i.LocationId == entity.Id));

                return !usedInItems;
            }
            catch
            {
                return false;
            }
        }
    }
}