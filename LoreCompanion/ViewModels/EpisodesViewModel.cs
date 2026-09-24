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

        protected override Episode CreateEntityInstance()
        {
            var max = Items.Select(item => item.GetEpisodeNumber()).DefaultIfEmpty(0).Max();

            return new Episode { Name = $"Episode {max + 1}" };
        }

        protected override bool FilterEntity(Episode entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<bool> CanDeleteAsync(LoreDbContext context, Episode entity)
        {
            try
            {
                var episodeInUse =
                    await context.Items.AnyAsync(i => (i.Episode != null) && (i.Episode.Id == entity.Id));

                return !episodeInUse;
            }
            catch
            {
                return false;
            }
        }
    }
}