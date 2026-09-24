using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Events;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;
using R3;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public abstract class MasterDetailSectionScreen<TEntity> : SectionScreen, IHandle<DatabaseUpdatedEvent>
        where TEntity : EntityBase, IEditableObject, new()
    {
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly IEventAggregator _eventAggregator;

        private IDisposable? _subscription;
        private bool _databaseRefreshNeeded = true;
        private int _busyCount;
        private Task? _loadingTask;
        private CancellationTokenSource? _loadingCts;

        protected MasterDetailSectionScreen(
            string section,
            IDbContextFactory<LoreDbContext> dbContextFactory,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
            : base(section)
        {
            DbContextFactory = dbContextFactory;
            _dialogService = dialogService;
            _notificationService = notificationService;
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            ItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = OnFilter;
            ItemsView.CustomSort = Comparer<TEntity>.Create(CompareEntities);
        }

        protected event AsyncEventHandler<LoadEntitiesEventArgs> LoadingEntities = (_, _) => Task.CompletedTask;

        protected event AsyncEventHandler<EventArgs> LoadedEntities = (_, _) => Task.CompletedTask;

        protected event AsyncEventHandler<SaveEntityEventArgs<TEntity>> SavingEntity = (_, _) => Task.CompletedTask;

        protected event AsyncEventHandler<SaveEntityEventArgs<TEntity>> SavedEntity = (_, _) => Task.CompletedTask;

        [UsedImplicitly]
        public BindableCollection<TEntity> Items { get; } = [];

        public TEntity? SelectedItem
        {
            get;
            set
            {
                var previousItem = SelectedItem;

                if (!Set(ref field, value))
                {
                    return;
                }

                if ((EditMode != EditMode.Edit) || previousItem is null)
                {
                    return;
                }

                EditMode = EditMode.ReadOnly;

                if (Items.Contains(previousItem))
                {
                    _ = SaveItemAsync(previousItem);
                }
            }
        }

        public ListCollectionView ItemsView { get; }

        public string SearchText
        {
            get;
            set => Set(ref field, value);
        } = "";

        public EditMode EditMode
        {
            get;
            private set
            {
                if ((value == EditMode.Edit) && !AppHelper.IsAdminMode)
                {
                    Logger.Error("Cannot edit entity in non-admin mode");

                    return;
                }

                Set(ref field, value);
            }
        } = EditMode.ReadOnly;

        public bool IsBusy => _busyCount > 0;

        protected IDbContextFactory<LoreDbContext> DbContextFactory { get; }

        protected ILogger Logger => field ??= LogManager.GetLogger(GetType());

        private SemaphoreSlim DatabaseLock { get; } = new(1, 1);

        [PublicAPI]
        public Task CreateNewAsync()
        {
            if (!AppHelper.IsAdminMode)
            {
                Logger.Error("Cannot create entity in non-admin mode");

                return Task.CompletedTask;
            }

            var newEntity = CreateEntityInstance();
            Items.Add(newEntity);
            SelectedItem = newEntity;

            Logger.Debug("New entity created");

            // Immediately switch to editable mode for the new entity
            newEntity.BeginEdit();
            EditMode = EditMode.Edit;

            return Task.CompletedTask;
        }

        [PublicAPI]
        public void EditCurrentAsync()
        {
            if (!AppHelper.IsAdminMode)
            {
                Logger.Error("Cannot edit entity in non-admin mode");

                return;
            }

            if (SelectedItem is not null)
            {
                SelectedItem.BeginEdit();
                EditMode = EditMode.Edit;
            }
        }

        [PublicAPI]
        public async Task SaveCurrentAsync()
        {
            if (!AppHelper.IsAdminMode)
            {
                Logger.Error("Cannot save entity in non-admin mode");

                return;
            }

            if (SelectedItem is not null)
            {
                EditMode = EditMode.ReadOnly;
                await SaveItemAsync(SelectedItem);
            }
        }

        [PublicAPI]
        public void RollbackCurrent()
        {
            if (SelectedItem is null)
            {
                return;
            }

            var current = SelectedItem;
            EditMode = EditMode.ReadOnly;
            SelectedItem.CancelEdit();

            if (current.Id == 0)
            {
                RemoveItemAndUpdateSelection(current);
            }
        }

        [PublicAPI]
        public async Task DeleteAsync(TEntity entity)
        {
            if (!AppHelper.IsAdminMode)
            {
                Logger.Error("Cannot delete entity in non-admin mode");

                return;
            }

            var dialogResult = await _dialogService.ShowQueryDialogAsync(
                                   $"Delete {entity.GetType().Name}",
                                   $"Are you sure you want to delete this {entity.GetType().Name.ToLower()}?\n\n'{entity}'",
                                   DialogResults.YesNo,
                                   DialogResult.Yes);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            await DatabaseLock.WaitAsync();

            try
            {
                if (entity.Id == 0)
                {
                    // New entity: no action required
                }
                else
                {
                    // Existing entity: update database record
                    Logger.Debug(
                        "Deleting {EntityName} '{Entity}' from database...",
                        entity.GetType().Name.ToLower(),
                        entity);

                    await using var context = await DbContextFactory.CreateDbContextAsync();

                    var canDelete = await CanDeleteAsync(context, entity);

                    if (!canDelete)
                    {
                        _ = _notificationService.ShowNotificationAsync(
                            "Deletion failed",
                            $"Deleting {entity.GetType().Name.ToLower()} '{entity}' not possible.\n\nIt is referenced by other entries.",
                            NotificationType.Error);

                        return;
                    }

                    context.Set<TEntity>().Remove(entity);
                    await context.SaveChangesAsync();
                }

                RemoveItemAndUpdateSelection(entity);

                await _eventAggregator.PublishOnUIThreadAsync(new EntityUpdatedEvent<TEntity>());

                _ = _notificationService.ShowNotificationAsync(
                    $"{entity.GetType().Name} deleted",
                    $"{entity.GetType().Name} '{entity}' was deleted successfully.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error deleting {EntityName} '{Entity}'", entity.GetType().Name.ToLower(), entity);

                _ = _notificationService.ShowNotificationAsync(
                    "Deletion failed",
                    $"Could not delete {entity.GetType().Name.ToLower()} '{entity}'.\n{e.Message}",
                    NotificationType.Error);
            }
            finally
            {
                DatabaseLock.Release();
            }
        }

        public Task HandleAsync(DatabaseUpdatedEvent message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected virtual Task<bool> CanDeleteAsync(LoreDbContext context, TEntity entity)
        {
            return Task.FromResult(true);
        }

        protected virtual IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return context.Set<TEntity>();
        }

        protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            Logger.Debug("Activated");

            _subscription?.Dispose();

            _subscription = this.ObservePropertyChanged(x => x.SearchText)
                                .DistinctUntilChanged()
                                .Debounce(TimeSpan.FromMilliseconds(250))
                                .ObserveOnCurrentDispatcher()
                                .Subscribe(_ => ApplyFilterAndSyncSelection());

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                ApplyFilterAndSyncSelection();
            }

            if (_loadingCts is not null)
            {
                await _loadingCts.CancelAsync();
                _loadingCts.Dispose();
            }

            _loadingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _loadingTask = Task.Run(() => LoadEntitiesAsync(_loadingCts.Token), _loadingCts.Token);

            await base.OnActivatedAsync(cancellationToken);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Logger.Debug("Deactivating...");

            if (_loadingCts is not null)
            {
                await _loadingCts.CancelAsync();
                _loadingCts.Dispose();
                _loadingCts = null;
            }

            _subscription?.Dispose();
            _subscription = null;

            if ((EditMode == EditMode.Edit) && SelectedItem is not null)
            {
                try
                {
                    await SaveCurrentAsync();
                }
                catch (Exception e)
                {
                    // Log and ignore cancellation to ensure cleanup proceeds
                    Logger.Error(e, "Error saving current entity during deactivation");

                    _ = _notificationService.ShowNotificationAsync(
                        "Database error",
                        $"Could not save {SelectedItem.GetType().Name.ToLower()} '{SelectedItem}'.\n{e.Message}",
                        NotificationType.Error,
                        cancellationToken: CancellationToken.None);
                }
            }

            var lockAcquired = false;

            try
            {
                await DatabaseLock.WaitAsync(cancellationToken);
                lockAcquired = true;
            }
            catch (OperationCanceledException e)
            {
                // Log and ignore cancellation to ensure cleanup proceeds
                Logger.Error(e, "Database lock wait cancelled during deactivation");

                _ = _notificationService.ShowNotificationAsync(
                    "Database error",
                    $"Could not finish database operation.\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);
            }
            finally
            {
                if (lockAcquired)
                {
                    DatabaseLock.Release();
                }
            }

            if (close)
            {
                if (_loadingTask is not null)
                {
                    try
                    {
                        await _loadingTask;
                    }
                    catch (OperationCanceledException e)
                    {
                        // Log and ignore cancellation to ensure cleanup proceeds
                        Logger.Debug(e, "Loading task cancelled during deactivation");
                    }
                }
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        protected abstract int CompareEntities(TEntity x, TEntity y);

        protected abstract TEntity CreateEntityInstance();

        protected abstract bool FilterEntity(TEntity entity, string searchText);

        protected ActionDisposable SetBusy()
        {
            if (Interlocked.Increment(ref _busyCount) == 1)
            {
                NotifyOfPropertyChange(nameof(IsBusy));
            }

            return new ActionDisposable(() =>
            {
                if (Interlocked.Decrement(ref _busyCount) == 0)
                {
                    NotifyOfPropertyChange(nameof(IsBusy));
                }
            });
        }

        private async Task LoadEntitiesAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();

            await Task.Yield();
            var lockAcquired = false;
            TEntity? currentSelectedItem = null;

            try
            {
                Logger.Debug("Loading items...");

                Execute.OnUIThread(() =>
                {
                    currentSelectedItem = SelectedItem;
                    SelectedItem = null;
                });

                await DatabaseLock.WaitAsync(cancellationToken);
                lockAcquired = true;

                if (_databaseRefreshNeeded)
                {
                    await Task.WhenAll(
                        LoadEntitiesInternalAsync(cancellationToken),
                        NotifyOfLoadingEntities(true, cancellationToken));

                    _databaseRefreshNeeded = false;
                }
                else
                {
                    await NotifyOfLoadingEntities(false, cancellationToken);
                }

                await LoadedEntities.InvokeAllAsync(this, EventArgs.Empty);
            }
            catch (OperationCanceledException e)
            {
                Logger.Debug(e, "Loading task cancelled");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error loading items");

                _ = _notificationService.ShowNotificationAsync(
                    "Database error",
                    $"Could not load items.\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);
            }
            finally
            {
                Execute.OnUIThread(() =>
                {
                    var firstItem = default(TEntity);
                    var selectedItem = default(TEntity);

                    foreach (var item in ItemsView.Cast<TEntity>())
                    {
                        firstItem ??= item;

                        if (item.Id == currentSelectedItem?.Id)
                        {
                            selectedItem = item;

                            break;
                        }
                    }

                    SelectedItem = selectedItem ?? firstItem;
                });

                if (lockAcquired)
                {
                    DatabaseLock.Release();
                }
            }
        }

        private async Task NotifyOfLoadingEntities(bool forceLoad, CancellationToken cancellationToken)
        {
            var args = new LoadEntitiesEventArgs { ForceLoad = forceLoad, CancellationToken = cancellationToken };

            await LoadingEntities.InvokeAllAsync(this, args);
        }

        private async Task LoadEntitiesInternalAsync(CancellationToken cancellationToken)
        {
            Execute.OnUIThread(() => { Items.Clear(); });

            await using (var context = await DbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                const int BatchSize = 25;
                var buffer = new List<TEntity>(BatchSize);

                // Stream items asynchronously from the database
                await foreach (var entity in GetAllItemsQuery(context)
                                             .AsNoTracking()
                                             .AsAsyncEnumerable()
                                             .WithCancellation(cancellationToken))
                {
                    buffer.Add(entity);

                    if (buffer.Count >= BatchSize)
                    {
                        UpdateItems(buffer);

                        // Yield to let the WPF Dispatcher render the newly added items
                        await Task.Yield();
                    }
                }

                if (buffer.Count > 0)
                {
                    UpdateItems(buffer);
                }
            }

            Logger.Debug("Items and auxiliary data loaded");
        }

        private async Task SaveItemAsync(TEntity entity)
        {
            if (!AppHelper.IsAdminMode)
            {
                Logger.Error("Cannot save entity in non-admin mode");

                return;
            }

            await DatabaseLock.WaitAsync();

            try
            {
                Logger.Debug("Saving {EntityName} '{Entity}' to database...", entity.GetType().Name.ToLower(), entity);
                await using var context = await DbContextFactory.CreateDbContextAsync();

                var saveEventArgs = new SaveEntityEventArgs<TEntity> { Entity = entity };
                await SavingEntity.InvokeAllAsync(this, saveEventArgs);

                if (entity.Id == 0)
                {
                    // New entity: insert into database
                    context.Set<TEntity>().Add(entity);
                }
                else
                {
                    // Existing entity: update database record
                    context.Set<TEntity>().Update(entity);
                }

                await context.SaveChangesAsync();
                await SavedEntity.InvokeAllAsync(this, saveEventArgs);
                await _eventAggregator.PublishOnUIThreadAsync(new EntityUpdatedEvent<TEntity>());

                entity.EndEdit();
                ItemsView.Refresh();

                _ = _notificationService.ShowNotificationAsync(
                    $"{entity.GetType().Name} saved",
                    $"{entity.GetType().Name} '{entity}' was saved successfully.");
            }
            catch (Exception e)
            {
                Logger.Error(
                    e,
                    "Error saving {EntityName} '{Entity}' to database",
                    entity.GetType().Name.ToLower(),
                    entity);

                entity.CancelEdit();

                if (entity.Id == 0)
                {
                    RemoveItemAndUpdateSelection(entity);
                }

                _ = _notificationService.ShowNotificationAsync(
                    "Save failed",
                    $"Could not save {entity.GetType().Name.ToLower()} '{entity}'.\n{e.Message}",
                    NotificationType.Error);
            }
            finally
            {
                DatabaseLock.Release();
            }
        }

        private void RemoveItemAndUpdateSelection(TEntity entity)
        {
            Execute.OnUIThread(() =>
            {
                var wasSelectedItem = ReferenceEquals(SelectedItem, entity);
                TEntity? nextSelection = null;

                if (wasSelectedItem)
                {
                    TEntity? previous = null;
                    var found = false;

                    foreach (var item in ItemsView)
                    {
                        if (item is not TEntity current)
                        {
                            continue;
                        }

                        if (found)
                        {
                            nextSelection = current; // First item after the removed entity

                            break;
                        }

                        if (ReferenceEquals(current, entity))
                        {
                            found = true;
                        }
                        else
                        {
                            previous = current; // Last item before the removed entity
                        }
                    }

                    // If there is no item after, fall back to the item before
                    nextSelection ??= previous;
                }

                Debug.Assert(!ReferenceEquals(nextSelection, entity));

                Items.Remove(entity);
                Logger.Debug("{EntityName} '{Entity}' removed from UI collection", entity.GetType().Name, entity);

                if (wasSelectedItem)
                {
                    SelectedItem = nextSelection;
                }
            });
        }

        private void ApplyFilterAndSyncSelection()
        {
            ItemsView.Refresh();

            // Keep current selection if it still satisfies the filter; otherwise select the first match
            if (SelectedItem is null || !ItemsView.Contains(SelectedItem))
            {
                SelectedItem = ItemsView.Cast<TEntity>().FirstOrDefault();
            }
        }

        private void UpdateItems(List<TEntity> buffer)
        {
            var chunk = buffer.ToArray();
            buffer.Clear();

            Execute.OnUIThread(() => { Items.AddRange(chunk); });
        }

        private bool OnFilter(object obj)
        {
            if (obj is not TEntity entity || string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            return FilterEntity(entity, SearchText);
        }
    }
}