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
        protected override Location CreateEntityInstance()
        {
            return new Location { Name = "New Location" };
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

        protected override async Task<IEnumerable<EntityBase>> GetRelatedItemsAsync(
            Location selectedItem,
            CancellationToken cancellationToken)
        {
            Logger.Debug("Loading related characters and items...");

            await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

            var characters = await context.Characters.Where(c => c.LocationId == selectedItem.Id)
                                          .OrderBy(c => c.Name)
                                          .AsNoTracking()
                                          .ToListAsync(cancellationToken);

            var items = await context.Items.Where(it => it.LocationId == selectedItem.Id)
                                     .OrderBy(it => it.Type)
                                     .ThenBy(it => it.Name)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);

            return characters.Cast<EntityBase>().Concat(items);
        }
    }
}