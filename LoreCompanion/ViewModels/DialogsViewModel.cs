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
    }
}