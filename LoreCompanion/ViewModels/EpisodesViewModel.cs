using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public sealed class EpisodesViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailSectionScreen<Episode>(
        NavigationSection.Progress,
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override Episode CreateEntityInstance()
        {
            var max = Items.Select(ep => ep.Number).DefaultIfEmpty(0).Max();

            return new Episode { Number = max + 1 };
        }

        protected override async Task<bool> CanDeleteAsync(LoreDbContext context, Episode entity)
        {
            try
            {
                var inUse = await context.Items.AnyAsync(i => (i.EpisodeId != null) && (i.EpisodeId == entity.Id));

                if (inUse)
                {
                    return false;
                }

                inUse = await context.Locations.AnyAsync(i => (i.EpisodeId != null) && (i.EpisodeId == entity.Id));

                if (inUse)
                {
                    return false;
                }

                inUse = await context.Characters.AnyAsync(i => (i.EpisodeId != null) && (i.EpisodeId == entity.Id));

                if (inUse)
                {
                    return false;
                }

                inUse = await context.Dialogs.AnyAsync(i => (i.EpisodeId != null) && (i.EpisodeId == entity.Id));

                return !inUse;
            }
            catch
            {
                return false;
            }
        }
    }
}