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
    public class ItemsViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeSectionScreen<Item>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        public void PlayVideo(Item item)
        {
            if (item.Episode is null)
            {
                return;
            }

            Logger.Debug("Playing video for item: {Item}", item.Name);

            var videoUrl = string.Format(
                YouTubeHelper.VideoUrlFormatStringWithTime,
                item.Episode.VideoKey,
                item.Timestamp.TotalSeconds);

            Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
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