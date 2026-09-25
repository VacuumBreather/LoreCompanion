using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public sealed class CharactersViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeLocationSectionScreen<Character>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override Character CreateEntityInstance()
        {
            return new Character { Name = "New Character" };
        }

        protected override bool FilterEntity(Character entity, string searchText)
        {
            return entity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<bool> CanDeleteAsync(LoreDbContext context, Character entity)
        {
            try
            {
                var inUse = await context.Dialogs.AnyAsync(i => (i.CharacterId != null) &&
                                                                (i.CharacterId == entity.Id));

                return !inUse;
            }
            catch
            {
                return false;
            }
        }
    }
}