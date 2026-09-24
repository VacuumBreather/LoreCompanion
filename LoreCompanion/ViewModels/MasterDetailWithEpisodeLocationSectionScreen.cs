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
        where TEntity : EntityBase, INamed, IEpisodeReferencing, ILocationReferencing, IEditableObject, new()
    {
        private bool _locationsRefreshNeeded = true;

        protected MasterDetailWithEpisodeLocationSectionScreen(
            IDbContextFactory<LoreDbContext> dbContextFactory,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
            : base(dbContextFactory, dialogService, notificationService, eventAggregator)
        {
            LoadingEntities += OnLoadingEntities;
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

        private async Task OnLoadingEntities(object sender, LoadingEntitiesEventArgs e)
        {
            if (!_locationsRefreshNeeded && !e.ForceLoad)
            {
                return;
            }

            _locationsRefreshNeeded = false;

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
                }
                finally
                {
                    Locations.IsNotifying = true;
                    Locations.Refresh();
                }
            });
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