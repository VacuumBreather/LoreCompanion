using System.Diagnostics;
using Caliburn.Micro;
using JetBrains.Annotations;
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
            Logger.Debug("Playing video for episode '{EpisodeName}'", episode.ToString());
            var videoUrl = string.Format(YouTubeHelper.VideoUrlFormatString, episode.VideoKey);
            Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
        }

        protected override int CompareEntities(Episode x, Episode y)
        {
            return Comparer<int>.Default.Compare(x.Number, y.Number);
        }

        protected override Episode CreateEntityInstance()
        {
            var max = Items.Select(ep => ep.Number).DefaultIfEmpty(0).Max();

            return new Episode { Number = max + 1 };
        }

        protected override bool FilterEntity(Episode entity, string searchText)
        {
            return entity.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase);
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