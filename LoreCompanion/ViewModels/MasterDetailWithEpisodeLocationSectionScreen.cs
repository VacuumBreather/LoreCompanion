using System.ComponentModel;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    public abstract class MasterDetailWithEpisodeLocationSectionScreen<TEntity>(
        IDbContextFactory<LoreDbContext> dbContextFactory,
        IDialogService dialogService,
        INotificationService notificationService,
        IEventAggregator eventAggregator) : MasterDetailWithEpisodeSectionScreen<TEntity>(
                                                dbContextFactory,
                                                dialogService,
                                                notificationService,
                                                eventAggregator),
                                            IHandle<LocationsUpdatedEvent>
        where TEntity : EntityBase, INamed, IEpisodeReferencing, ILocationReferencing, IEditableObject, new()
    {
        private bool _locationsRefreshNeeded = true;

        [UsedImplicitly]
        public BindableCollection<Location> Locations { get; } = new();

        public Task HandleAsync(LocationsUpdatedEvent message, CancellationToken cancellationToken)
        {
            _locationsRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected virtual async Task LoadLocationsAsync(CancellationToken cancellationToken)
        {
            await using var context = await DbContextFactory.CreateDbContextAsync(cancellationToken);

            var locations = await context.Locations.AsNoTracking().OrderBy(l => l.Name).ToListAsync(cancellationToken);

            Execute.OnUIThread(() =>
            {
                try
                {
                    Locations.IsNotifying = false;
                    Locations.Clear();
                    Locations.AddRange(locations);
                }
                finally
                {
                    Locations.IsNotifying = true;
                    Locations.Refresh();
                }
            });
        }

        protected override IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return base.GetAllItemsQuery(context).Include(x => x.Location);
        }

        protected override void ClearAdditional()
        {
            Locations.Clear();
            base.ClearAdditional();
        }

        protected override Task LoadAdditionalAsync(CancellationToken cancellationToken)
        {
            _locationsRefreshNeeded = false;

            return Task.WhenAll(base.LoadAdditionalAsync(cancellationToken), LoadLocationsAsync(cancellationToken));
        }

        protected override async Task ReloadAuxiliaryDataAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();
            await Task.Yield();

            try
            {
                await DatabaseLock.WaitAsync(cancellationToken);

                var tasks = new List<Task>();

                if (_locationsRefreshNeeded)
                {
                    tasks.Add(LoadLocationsAsync(cancellationToken));
                }

                tasks.Add(base.LoadEpisodesAsync(cancellationToken));

                await Task.WhenAll(tasks);
                _locationsRefreshNeeded = false;

                // Re-link navigation references on existing items
                Execute.OnUIThread(() =>
                {
                    var episodeLookup = Episodes.ToDictionary(e => e.Id);
                    var locationLookup = Locations.ToDictionary(l => l.Id);

                    foreach (var item in Items)
                    {
                        item.Episode =
                            item.EpisodeId.HasValue && episodeLookup.TryGetValue(item.EpisodeId.Value, out var ep)
                                ? ep
                                : null;

                        item.Location =
                            item.LocationId.HasValue && locationLookup.TryGetValue(item.LocationId.Value, out var loc)
                                ? loc
                                : null;
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

        protected override Task BeforeSaveAsync(TEntity entity, LoreDbContext context)
        {
            entity.Location = null;

            return base.BeforeSaveAsync(entity, context);
        }

        protected override Task AfterSaveAsync(TEntity entity, LoreDbContext context)
        {
            entity.Location = entity.LocationId.HasValue
                                  ? Locations.FirstOrDefault(e => e.Id == entity.LocationId.Value)
                                  : null;

            return base.AfterSaveAsync(entity, context);
        }
    }
}