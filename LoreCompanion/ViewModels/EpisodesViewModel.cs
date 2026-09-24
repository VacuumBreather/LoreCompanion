using System.Diagnostics;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Extensions;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public class EpisodesViewModel(
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
        [UsedImplicitly]
        public void PlayVideo(Episode episode)
        {
            Logger.Debug("Playing video for episode: {Episode}", episode.Name);
            var videoUrl = string.Format(YouTubeHelper.VideoUrlFormatString, episode.VideoKey);
            Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
        }

        protected override int CompareEntities(Episode x, Episode y)
        {
            return Comparer<int>.Default.Compare(x.GetEpisodeNumber(), y.GetEpisodeNumber());
        }

        protected override Episode CreateEntityInstance()
        {
            var max = Items.Select(item => item.GetEpisodeNumber()).DefaultIfEmpty(0).Max();

            return new Episode { Name = $"Episode {max + 1}" };
        }

        protected override async Task OnEntitySavedAsync(Episode entity)
        {
            await EventAggregator.PublishOnUIThreadAsync(new EpisodesUpdatedEvent());
        }

        protected override async Task OnEntityDeletedAsync(Episode entity)
        {
            await EventAggregator.PublishOnUIThreadAsync(new EpisodesUpdatedEvent());
        }

        protected override bool FilterEntity(Episode entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<bool> CanDeleteAsync(LoreDbContext context, Episode entity)
        {
            try
            {
                var usedInItems =
                    await context.Items.AnyAsync(i => (i.EpisodeId != null) && (i.EpisodeId == entity.Id));

                if (usedInItems)
                {
                    return false;
                }

                var usedInLocations =
                    await context.Locations.AnyAsync(i => (i.EpisodeId != null) && (i.EpisodeId == entity.Id));

                return !usedInLocations;
            }
            catch
            {
                return false;
            }
        }
    }
}