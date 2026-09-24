using System.ComponentModel;
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
    public abstract class MasterDetailWithEpisodeSectionScreen<TEntity>(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailSectionScreen<TEntity>(
                                                NavigationSection.Lore,
                                                dbContextFactory,
                                                dialogService,
                                                notificationService,
                                                eventAggregator),
                                            IHandle<EpisodesUpdatedEvent>
        where TEntity : EntityBase, INamed, IEpisodeReferencing, IEditableObject, new()
    {
        private bool _episodesRefreshNeeded = true;

        [UsedImplicitly]
        public BindableCollection<Episode> Episodes { get; } = new();

        [UsedImplicitly]
        public void PlayVideo(TEntity entity)
        {
            if (entity.Episode is null)
            {
                return;
            }

            Logger.Debug("Playing video for {EntityName}: {Location}", entity.GetType().Name.ToLower(), entity.Name);

            var videoUrl = string.Format(
                YouTubeHelper.VideoUrlFormatStringWithTime,
                entity.Episode.VideoKey,
                entity.Timestamp.TotalSeconds);

            Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
        }

        public Task HandleAsync(EpisodesUpdatedEvent message, CancellationToken cancellationToken)
        {
            _episodesRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected virtual async Task ReloadAuxiliaryDataAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();
            await Task.Yield();

            try
            {
                await DatabaseLock.WaitAsync(cancellationToken);

                await LoadEpisodesAsync(cancellationToken);
                _episodesRefreshNeeded = false;

                // Reconcile existing in-memory items with the newly loaded Episode instances
                Execute.OnUIThread(() =>
                {
                    var lookup = Episodes.ToDictionary(e => e.Id);

                    foreach (var item in Items)
                    {
                        if (item.EpisodeId.HasValue && lookup.TryGetValue(item.EpisodeId.Value, out var ep))
                        {
                            item.Episode = ep;
                        }
                        else
                        {
                            item.Episode = null;
                        }
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                DatabaseLock.Release();
            }
        }

        protected virtual async Task LoadEpisodesAsync(CancellationToken cancellationToken)
        {
            await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

            var episodes = await context.Episodes.AsNoTracking().ToListAsync(cancellationToken);

            episodes.Sort((a, b) => a.GetEpisodeNumber().CompareTo(b.GetEpisodeNumber()));

            Execute.OnUIThread(() =>
            {
                try
                {
                    Episodes.IsNotifying = false;
                    Episodes.Clear();
                    Episodes.AddRange(episodes);
                }
                finally
                {
                    Episodes.IsNotifying = true;
                    Episodes.Refresh();
                }
            });
        }

        protected override IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return context.Set<TEntity>().Include(x => x.Episode);
        }

        protected override void ClearAdditional()
        {
            Episodes.Clear();
        }

        protected override async Task PerformDataLoadAsync(CancellationToken cancellationToken)
        {
            await base.PerformDataLoadAsync(cancellationToken);

            // If a full reload did not happen, but episodes changed, reload auxiliary data selectively
            if (_episodesRefreshNeeded)
            {
                await ReloadAuxiliaryDataAsync(cancellationToken);
            }
        }

        protected override Task LoadAdditionalAsync(CancellationToken cancellationToken)
        {
            _episodesRefreshNeeded = false;

            return LoadEpisodesAsync(cancellationToken);
        }

        protected override Task BeforeSaveAsync(TEntity entity, LoreDbContext context)
        {
            // Clear navigation reference to prevent EF Core graph/tracking conflicts
            entity.Episode = null;

            return Task.CompletedTask;
        }

        protected override Task AfterSaveAsync(TEntity entity, LoreDbContext context)
        {
            // Re-link navigation reference so UI (Episode.Name) and PlayVideo have the active Episode instance
            entity.Episode = entity.EpisodeId.HasValue
                                 ? Episodes.FirstOrDefault(e => e.Id == entity.EpisodeId.Value)
                                 : null;

            return Task.CompletedTask;
        }
    }
}