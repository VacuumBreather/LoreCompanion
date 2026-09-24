using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public sealed class LocationsViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeSectionScreen<Location>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override int CompareEntities(Location x, Location y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

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
                var inUse = await context.Items.AnyAsync(i => (i.LocationId != null) && (i.LocationId == entity.Id));

                if (inUse)
                {
                    return false;
                }

                inUse = await context.Characters.AnyAsync(i => (i.LocationId != null) && (i.LocationId == entity.Id));

                if (inUse)
                {
                    return false;
                }

                inUse = await context.Dialogs.AnyAsync(i => (i.LocationId != null) && (i.LocationId == entity.Id));

                return !inUse;
            }
            catch
            {
                return false;
            }
        }
    }
}