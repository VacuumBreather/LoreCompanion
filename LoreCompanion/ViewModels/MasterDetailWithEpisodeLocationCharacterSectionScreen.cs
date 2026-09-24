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
    public abstract class MasterDetailWithEpisodeLocationCharacterSectionScreen<TEntity> :
        MasterDetailWithEpisodeLocationSectionScreen<TEntity>,
        IHandle<EntityUpdatedEvent<Character>>
        where TEntity : EntityBase, ICharacterReferencing, IEpisodeReferencing, ILocationReferencing, IEditableObject,
        new()
    {
        private bool _charactersRefreshNeeded = true;
        private bool _needCharactersReconciliation;

        protected MasterDetailWithEpisodeLocationCharacterSectionScreen(
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
        public BindableCollection<Character> Characters { get; } = [];

        public Task HandleAsync(EntityUpdatedEvent<Character> message, CancellationToken cancellationToken)
        {
            _charactersRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected override IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return base.GetAllItemsQuery(context).Include(x => x.Character);
        }

        private async Task OnLoadingEntities(object sender, LoadEntitiesEventArgs e)
        {
            if (!_charactersRefreshNeeded && !e.ForceLoad)
            {
                return;
            }

            Logger.Debug("Loading characters...");

            _needCharactersReconciliation = true;

            await using var context = await DbContextFactory.CreateDbContextAsync(e.CancellationToken);

            var characters = await context.Characters.AsNoTracking()
                                          .OrderBy(l => l.Name)
                                          .ToListAsync(e.CancellationToken);

            Execute.OnUIThread(() =>
            {
                try
                {
                    Characters.IsNotifying = false;
                    Characters.Clear();
                    Characters.AddRange(characters);

                    _charactersRefreshNeeded = false;
                    Logger.Debug("Characters loaded");
                }
                finally
                {
                    Characters.IsNotifying = true;
                    Characters.Refresh();
                }
            });
        }

        private Task OnLoadedEntities(object sender, EventArgs e)
        {
            if (!_needCharactersReconciliation)
            {
                return Task.CompletedTask;
            }

            Logger.Debug("Reconciling characters...");

            Execute.OnUIThread(() =>
            {
                var lookUp = Characters.ToDictionary(c => c.Id);

                foreach (var item in Items)
                {
                    item.Character =
                        item.CharacterId.HasValue && lookUp.TryGetValue(item.CharacterId.Value, out var character)
                            ? character
                            : null;
                }
            });

            _needCharactersReconciliation = false;

            Logger.Debug("Characters reconciled");

            return Task.CompletedTask;
        }

        private Task OnSavingEntity(object sender, SaveEntityEventArgs<TEntity> args)
        {
            // Clear navigation reference to prevent EF Core graph/tracking conflicts
            args.Entity.Character = null;

            return Task.CompletedTask;
        }

        private Task OnSavedEntity(object sender, SaveEntityEventArgs<TEntity> args)
        {
            // Re-link navigation reference
            args.Entity.Character = args.Entity.CharacterId.HasValue
                                        ? Characters.FirstOrDefault(e => e.Id == args.Entity.CharacterId.Value)
                                        : null;

            return Task.CompletedTask;
        }
    }
}