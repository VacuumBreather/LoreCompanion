using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
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
        protected override Episode CreateEntityInstance()
        {
            var max = Items.Select(item => item.Name.Replace("Episode", "").Trim())
                           .Select(name => int.TryParse(name, out var number) ? number : 0)
                           .DefaultIfEmpty(0)
                           .Max();

            return new Episode { Name = $"Episode {max + 1}" };
        }

        protected override bool FilterEntity(Episode entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}