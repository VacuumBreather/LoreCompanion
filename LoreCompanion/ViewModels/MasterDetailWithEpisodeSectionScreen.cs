using System.ComponentModel;
using System.Diagnostics;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Events;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    public abstract class MasterDetailWithEpisodeSectionScreen<TEntity> : MasterDetailSectionScreen<TEntity>,
                                                                          IHandle<EntityUpdatedEvent<Episode>>
        where TEntity : EntityBase, IEpisodeReferencing, IEditableObject, new()
    {
        private bool _episodesRefreshNeeded = true;
        private bool _needEpisodesReconciliation;

        protected MasterDetailWithEpisodeSectionScreen(
            IDbContextFactory<LoreDbContext> dbContextFactory,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
            : base(
                NavigationSection.Lore,
                dbContextFactory,
                dialogService,
                notificationService,
                eventAggregator)
        {
            LoadingEntities += OnLoadingEntities;
            LoadedEntities += OnLoadedEntities;
            SavingEntity += OnSavingEntity;
            SavedEntity += OnSavedEntity;
        }

        [UsedImplicitly]
        public BindableCollection<Episode> Episodes { get; } = [];

        [UsedImplicitly]
        public void PlayVideo(TEntity entity)
        {
            if (entity.Episode is null)
            {
                return;
            }

            Logger.Debug("Playing video for {EntityName}: {Location}", entity.GetType().Name.ToLower(), entity);

            var videoUrl = string.Format(
                YouTubeHelper.VideoUrlFormatStringWithTime,
                entity.Episode.VideoKey,
                entity.Timestamp.TotalSeconds);

            Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
        }

        public Task HandleAsync(EntityUpdatedEvent<Episode> message, CancellationToken cancellationToken)
        {
            _episodesRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected override IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return context.Set<TEntity>().Include(x => x.Episode);
        }

        private async Task OnLoadingEntities(object sender, LoadEntitiesEventArgs e)
        {
            if (!_episodesRefreshNeeded && !e.ForceLoad)
            {
                return;
            }

            Logger.Debug("Loading episodes...");

            _needEpisodesReconciliation = true;

            await using var context = await DbContextFactory.CreateDbContextAsync(e.CancellationToken);

            var episodes = await context.Episodes.AsNoTracking()
                                        .OrderBy(x => x.Number)
                                        .ToListAsync(e.CancellationToken);

            Execute.OnUIThread(() =>
            {
                try
                {
                    Episodes.IsNotifying = false;
                    Episodes.Clear();
                    Episodes.AddRange(episodes);

                    _episodesRefreshNeeded = false;
                    Logger.Debug("Episodes loaded");
                }
                finally
                {
                    Episodes.IsNotifying = true;
                    Episodes.Refresh();
                }
            });
        }

        private Task OnLoadedEntities(object sender, EventArgs e)
        {
            if (!_needEpisodesReconciliation)
            {
                return Task.CompletedTask;
            }

            Logger.Debug("Reconciling episodes...");

            Execute.OnUIThread(() =>
            {
                var lookUp = Episodes.ToDictionary(ep => ep.Id);

                foreach (var item in Items)
                {
                    item.Episode = item.EpisodeId.HasValue && lookUp.TryGetValue(item.EpisodeId.Value, out var episode)
                                       ? episode
                                       : null;
                }
            });

            _needEpisodesReconciliation = false;

            Logger.Debug("Episodes reconciled");

            return Task.CompletedTask;
        }

        private Task OnSavingEntity(object sender, SaveEntityEventArgs<TEntity> args)
        {
            // Clear navigation reference to prevent EF Core graph/tracking conflicts
            args.Entity.Episode = null;

            return Task.CompletedTask;
        }

        private Task OnSavedEntity(object sender, SaveEntityEventArgs<TEntity> args)
        {
            // Re-link navigation reference
            args.Entity.Episode = args.Entity.EpisodeId.HasValue
                                      ? Episodes.FirstOrDefault(e => e.Id == args.Entity.EpisodeId.Value)
                                      : null;

            return Task.CompletedTask;
        }
    }
}