using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [UsedImplicitly]
    public sealed class DialogsViewModel(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeLocationCharacterSectionScreen<Dialog>(
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
    {
        protected override Dialog CreateEntityInstance()
        {
            return new Dialog { Context = "New Dialog Context" };
        }

        protected override bool FilterEntity(Dialog entity, string searchText)
        {
            return (entity.Character is not null &&
                    entity.Character.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                   entity.Context.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   entity.Content.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}