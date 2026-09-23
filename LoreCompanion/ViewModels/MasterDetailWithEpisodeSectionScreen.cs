using System.ComponentModel;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Extensions;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    public abstract class MasterDetailWithEpisodeSectionScreen<TEntity>(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailSectionScreen<TEntity>(
        NavigationSection.Lore,
        dbContextFactory,
        dialogService,
        notificationService,
        eventAggregator)
        where TEntity : EntityBase, INamed, IEpisodeReferencing, IEditableObject, new()
    {
        [UsedImplicitly]
        public BindableCollection<Episode> Episodes { get; } = new();

        protected override IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return context.Set<TEntity>().Include(x => x.Episode);
        }

        protected override void ClearAdditional()
        {
            Episodes.Clear();
        }

        protected override async Task UpdateAdditionalAsync(LoreDbContext context, CancellationToken cancellationToken)
        {
            try
            {
                Episodes.IsNotifying = false;

                await foreach (var episode in context.Episodes.AsNoTracking()
                                                     .AsAsyncEnumerable()
                                                     .OrderBy(ep => ep.GetEpisodeNumber())
                                                     .WithCancellation(cancellationToken))
                {
                    Episodes.Add(episode);
                }
            }
            finally
            {
                Episodes.Refresh();
                Episodes.IsNotifying = true;
            }
        }
    }
}