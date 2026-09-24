using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;
using R3;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public abstract class MasterDetailSectionScreen<TEntity> : SectionScreen, IHandle<DatabaseUpdatedEvent>
        where TEntity : EntityBase, INamed, IEditableObject, new()
    {
        private readonly IDbContextFactory<LoreDbContext> _dbContextFactory;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly SemaphoreSlim _databaseLock = new(1, 1);

        private IDisposable? _subscription;
        private bool _databaseRefreshNeeded = true;
        private int _busyCount;
        private Task? _loadingTask;
        private CancellationTokenSource? _loadingCts;

        private ILogger? _logger;

        protected MasterDetailSectionScreen(
            string section,
            IDbContextFactory<LoreDbContext> dbContextFactory,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
            : base(section)
        {
            _dbContextFactory = dbContextFactory;
            _dialogService = dialogService;
            _notificationService = notificationService;
            eventAggregator.SubscribeOnPublishedThread(this);

            ItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = OnFilter;
            ItemsView.CustomSort = Comparer<TEntity>.Create(CompareEntities);
        }

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

        protected ILogger Logger => _logger ??= LogManager.GetLogger(GetType());

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
                                   $"Are you sure you want to delete this {entity.GetType().Name.ToLower()}?\n\n'{entity.Name}'",
                                   DialogResults.YesNo,
                                   DialogResult.Yes);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            await _databaseLock.WaitAsync();

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

                    await using var context = await _dbContextFactory.CreateDbContextAsync();

                    var canDelete = await CanDeleteAsync(context, entity);

                    if (!canDelete)
                    {
                        _ = _notificationService.ShowNotificationAsync(
                            "Deletion failed",
                            $"Deleting {entity.GetType().Name.ToLower()} '{entity.Name}' not possible.\n\nIt is referenced by other entries.",
                            NotificationType.Error);

                        return;
                    }

                    context.Set<TEntity>().Remove(entity);
                    await context.SaveChangesAsync();
                }

                RemoveItemAndUpdateSelection(entity);

                _ = _notificationService.ShowNotificationAsync(
                    $"{entity.GetType().Name} deleted",
                    $"{entity.GetType().Name} '{entity.Name}' was deleted successfully.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error deleting {EntityName} '{Entity}'", entity.GetType().Name.ToLower(), entity);

                _ = _notificationService.ShowNotificationAsync(
                    "Deletion failed",
                    $"Could not delete {entity.GetType().Name.ToLower()} '{entity.Name}'.\n{e.Message}",
                    NotificationType.Error);
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        public Task HandleAsync(DatabaseUpdatedEvent message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected virtual int CompareEntities(TEntity x, TEntity y)
        {
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }

        protected virtual Task<bool> CanDeleteAsync(LoreDbContext context, TEntity entity)
        {
            return Task.FromResult(true);
        }

        protected virtual IQueryable<TEntity> GetAllItemsQuery(LoreDbContext context)
        {
            return context.Set<TEntity>();
        }

        protected virtual void ClearAdditional()
        {
        }

        protected virtual Task UpdateAdditionalAsync(LoreDbContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task BeforeSaveAsync(TEntity entity, LoreDbContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterSaveAsync(TEntity entity, LoreDbContext context)
        {
            return Task.CompletedTask;
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

            if (_databaseRefreshNeeded)
            {
                if (_loadingCts is not null)
                {
                    await _loadingCts.CancelAsync();
                    _loadingCts.Dispose();
                }

                _loadingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _loadingTask = Task.Run(() => LoadItemsProgressivelyAsync(_loadingCts.Token), _loadingCts.Token);
            }

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
                        $"Could not save {SelectedItem.GetType().Name.ToLower()} '{SelectedItem.Name}'.\n{e.Message}",
                        NotificationType.Error,
                        cancellationToken: CancellationToken.None);
                }
            }

            try
            {
                await _databaseLock.WaitAsync(cancellationToken);
                _databaseLock.Release();
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

        protected abstract TEntity CreateEntityInstance();

        protected abstract bool FilterEntity(TEntity entity, string searchText);

        private async Task LoadItemsProgressivelyAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();

            await Task.Yield();

            try
            {
                Logger.Debug("Loading items...");

                await _databaseLock.WaitAsync(cancellationToken);
                await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

                Execute.OnUIThread(() =>
                {
                    SelectedItem = null;
                    Items.Clear();
                    ClearAdditional();
                });

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
                        UpdateItemsAndSelectFirst(buffer);

                        // Yield to let the WPF Dispatcher render the newly added items
                        await Task.Yield();
                    }
                }

                if (buffer.Count > 0)
                {
                    UpdateItemsAndSelectFirst(buffer);
                }

                await UpdateAdditionalAsync(context, cancellationToken);

                _databaseRefreshNeeded = false;
                Logger.Debug("Items loaded");
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
                _databaseLock.Release();
            }
        }

        private async Task SaveItemAsync(TEntity entity)
        {
            if (!AppHelper.IsAdminMode)
            {
                Logger.Error("Cannot save entity in non-admin mode");

                return;
            }

            await _databaseLock.WaitAsync();

            try
            {
                Logger.Debug("Saving {EntityName} '{Entity}' to database...", entity.GetType().Name.ToLower(), entity);
                await using var context = await _dbContextFactory.CreateDbContextAsync();

                await BeforeSaveAsync(entity, context);

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

                await AfterSaveAsync(entity, context);

                entity.EndEdit();
                ItemsView.Refresh();

                _ = _notificationService.ShowNotificationAsync(
                    $"{entity.GetType().Name} saved",
                    $"{entity.GetType().Name} '{entity.Name}' was saved successfully.");
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
                    $"Could not save {entity.GetType().Name.ToLower()} '{entity.Name}'.\n{e.Message}",
                    NotificationType.Error);
            }
            finally
            {
                _databaseLock.Release();
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

        private void UpdateItemsAndSelectFirst(List<TEntity> buffer)
        {
            var chunk = buffer.ToArray();
            buffer.Clear();

            Execute.OnUIThread(() =>
            {
                Items.AddRange(chunk);
                SelectedItem ??= chunk.FirstOrDefault();
            });
        }

        private bool OnFilter(object obj)
        {
            if (obj is not TEntity entity || string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            return FilterEntity(entity, SearchText);
        }

        private ActionDisposable SetBusy()
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
    }
}