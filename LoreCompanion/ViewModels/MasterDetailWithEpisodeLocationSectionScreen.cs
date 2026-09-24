using System.ComponentModel;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Events;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    public abstract class
        MasterDetailWithEpisodeLocationSectionScreen<TEntity> : MasterDetailWithEpisodeSectionScreen<TEntity>,
                                                                IHandle<EntityUpdatedEvent<Location>>
        where TEntity : EntityBase, IEpisodeReferencing, ILocationReferencing, IEditableObject, new()
    {
        private bool _locationsRefreshNeeded = true;
        private bool _needLocationsReconciliation;

        protected MasterDetailWithEpisodeLocationSectionScreen(
            IDbContextFactory<LoreDbContext> dbContextFactory,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
            : base(dbContextFactory, dialogService, notificationService, eventAggregator)
        {
            LoadingEntities += OnLoadingEntities;
            LoadedEntities += OnLoadedEntities;
            SavingEntity += OnSavingEntity;
            SavedEntity += OnSavedEntity;
        }

        [UsedImplicitly]
        public BindableCollection<Location> Locations { get; } = [];

        public Task HandleAsync(EntityUpdatedEvent<Location> message, CancellationToken cancellationToken)
        {
            _locationsRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected override IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return base.GetAllItemsQuery(context).Include(x => x.Location);
        }

        private async Task OnLoadingEntities(object sender, LoadEntitiesEventArgs e)
        {
            if (!_locationsRefreshNeeded && !e.ForceLoad)
            {
                return;
            }

            Logger.Debug("Loading locations...");

            _needLocationsReconciliation = true;

            await using var context = await DbContextFactory.CreateDbContextAsync(e.CancellationToken);

            var locations = await context
                                  .Locations.AsNoTracking()
                                  .OrderBy(l => l.Name)
                                  .ToListAsync(e.CancellationToken);

            Execute.OnUIThread(() =>
            {
                try
                {
                    Locations.IsNotifying = false;
                    Locations.Clear();
                    Locations.AddRange(locations);

                    _locationsRefreshNeeded = false;
                    Logger.Debug("Locations loaded");
                }
                finally
                {
                    Locations.IsNotifying = true;
                    Locations.Refresh();
                }
            });
        }

        private Task OnLoadedEntities(object sender, EventArgs e)
        {
            if (!_needLocationsReconciliation)
            {
                return Task.CompletedTask;
            }

            Logger.Debug("Reconciling locations...");

            Execute.OnUIThread(() =>
            {
                var lookUp = Locations.ToDictionary(loc => loc.Id);

                foreach (var item in Items)
                {
                    item.Location =
                        item.LocationId.HasValue && lookUp.TryGetValue(item.LocationId.Value, out var location)
                            ? location
                            : null;
                }
            });

            _needLocationsReconciliation = false;

            Logger.Debug("Locations reconciled");

            return Task.CompletedTask;
        }

        private Task OnSavingEntity(object sender, SaveEntityEventArgs<TEntity> args)
        {
            // Clear navigation reference to prevent EF Core graph/tracking conflicts
            args.Entity.Location = null;

            return Task.CompletedTask;
        }

        private Task OnSavedEntity(object sender, SaveEntityEventArgs<TEntity> args)
        {
            // Re-link navigation reference
            args.Entity.Location = args.Entity.LocationId.HasValue
                                       ? Locations.FirstOrDefault(e => e.Id == args.Entity.LocationId.Value)
                                       : null;

            return Task.CompletedTask;
        }
    }
}