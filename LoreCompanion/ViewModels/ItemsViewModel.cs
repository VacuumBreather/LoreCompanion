using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using Microsoft.EntityFrameworkCore;
using R3;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by DI")]
    public sealed class ItemsViewModel : SectionScreen
    {
        private readonly IDbContextFactory<LoreDbContext> _dbContextFactory;
        private readonly IDialogService _dialogService;
        private readonly SemaphoreSlim _databaseLock = new(1, 1);

        private IDisposable? _subscription;
        private int _busyCount;
        private Task? _loadingTask;

        public ItemsViewModel(IDbContextFactory<LoreDbContext> dbContextFactory, IDialogService dialogService)
            : base(NavigationSection.Lore)
        {
            _dbContextFactory = dbContextFactory;
            _dialogService = dialogService;
            DisplayName = "Items";

            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = OnFilter;
        }

        public ICollectionView ItemsView { get; }

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
                if (!Set(ref field, value))
                {
                    return;
                }

                NotifyOfPropertyChange(nameof(CanEditCurrent));
                NotifyOfPropertyChange(nameof(CanSaveCurrent));
            }
        } = EditMode.ReadOnly;

        public bool CanEditCurrent => SelectedItem is not null && (EditMode == EditMode.ReadOnly);

        public bool CanSaveCurrent => SelectedItem is not null && (EditMode == EditMode.Editable);

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by the UI")]
        public BindableCollection<Item> Items { get; } = [];

        public Item? SelectedItem
        {
            get;
            set
            {
                var previousItem = SelectedItem;

                if (!Set(ref field, value))
                {
                    return;
                }

                NotifyOfPropertyChange(nameof(CanEditCurrent));
                NotifyOfPropertyChange(nameof(CanSaveCurrent));

                if ((EditMode == EditMode.Editable) && previousItem is not null)
                {
                    EditMode = EditMode.ReadOnly;

                    if (Items.Contains(previousItem))
                    {
                        _ = SaveItemAsync(previousItem);
                    }
                }
            }
        }

        public bool IsBusy => _busyCount > 0;

        private static ILogger Logger { get; } = LogManager.GetLogger();

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by the UI")]
        public Task CreateNewAsync()
        {
            var newItem = new Item { Name = "New Item", Description = "Item Description" };
            Items.Add(newItem);
            SelectedItem = newItem;

            Logger.Debug("New item created");

            // Immediately switch to editable mode for the new item
            EditMode = EditMode.Editable;

            return Task.CompletedTask;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by the UI")]
        public async Task DeleteAsync(Item item)
        {
            var dialogResult = await _dialogService.ShowQueryDialogAsync(
                                   "Delete Item",
                                   $"Are you sure you want to delete this item?\n\n'{item.Name}'",
                                   DialogResults.YesNo,
                                   DialogResult.Yes);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            await _databaseLock.WaitAsync();

            try
            {
                if (item.Id == 0)
                {
                    // New item: no action required
                }
                else
                {
                    // Existing item: update database record
                    Logger.Debug("Deleting item '{Item}' from database...", item);
                    await using var context = await _dbContextFactory.CreateDbContextAsync();
                    context.Items.Remove(item);
                    await context.SaveChangesAsync();
                }

                var oldIndex = Items.IndexOf(item);
                var wasSelectedItem = SelectedItem?.Id == item.Id;

                Items.Remove(item);
                Logger.Debug("Item '{Item}' removed from UI collection", item);

                if (wasSelectedItem)
                {
                    Item? newSelectedItem = null;

                    if (Items.Count > oldIndex)
                    {
                        newSelectedItem = Items[oldIndex];
                    }
                    else if ((oldIndex > 0) && (Items.Count > oldIndex - 1))
                    {
                        newSelectedItem = Items[oldIndex - 1];
                    }

                    SelectedItem = newSelectedItem;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error deleting item '{Item}'", item);
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by the UI")]
        public void EditCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            EditMode = EditMode.Editable;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by the UI")]
        public async Task SaveCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            EditMode = EditMode.ReadOnly;
            await SaveItemAsync(SelectedItem);
        }

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            Logger.Debug("Initialized");

            _subscription = this.ObservePropertyChanged(x => x.SearchText)
                                .Debounce(TimeSpan.FromMilliseconds(250))
                                .ObserveOnCurrentDispatcher()
                                .Subscribe(_ => ItemsView.Refresh());

            _loadingTask = Task.Run(() => LoadItemsProgressivelyAsync(cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Logger.Debug("Deactivating...");

            if ((EditMode == EditMode.Editable) && SelectedItem is not null)
            {
                try
                {
                    await SaveCurrentAsync();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error saving current item during deactivation");

                    // Suppress to ensure deactivation continues
                }
            }

            try
            {
                await _databaseLock.WaitAsync(cancellationToken);
                _databaseLock.Release();
            }
            catch (OperationCanceledException e)
            {
                // Gracefully log and ignore cancellation to ensure cleanup proceeds
                Logger.Error(e, "Database lock wait cancelled during deactivation");
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
                        // Ignore
                        Logger.Error(e, "Loading task cancelled during deactivation");
                    }
                }

                _subscription?.Dispose();
                _subscription = null;
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        private BusyScope SetBusy()
        {
            if (Interlocked.Increment(ref _busyCount) == 1)
            {
                NotifyOfPropertyChange(nameof(IsBusy));
            }

            return new BusyScope(() =>
            {
                if (Interlocked.Decrement(ref _busyCount) == 0)
                {
                    NotifyOfPropertyChange(nameof(IsBusy));
                }
            });
        }

        private async Task LoadItemsProgressivelyAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();

            await Task.Yield();

            try
            {
                Logger.Debug("Loading items...");

                await _databaseLock.WaitAsync(cancellationToken);
                await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

                Execute.OnUIThread(() => Items.Clear());

                const int BatchSize = 25;
                var buffer = new List<Item>(BatchSize);

                // Stream items asynchronously from the database
                await foreach (var item in context.Items.AsNoTracking()
                                                  .AsAsyncEnumerable()
                                                  .WithCancellation(cancellationToken))
                {
                    buffer.Add(item);

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

                Logger.Debug("Items loaded");
            }
            catch (OperationCanceledException e)
            {
                // Expected when navigating away; terminate stream cleanly
                Logger.Debug(e, "Loading task cancelled");
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        private void UpdateItemsAndSelectFirst(List<Item> buffer)
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
            if (obj is not Item item || string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            return item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                   item.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        private async Task SaveItemAsync(Item item)
        {
            await _databaseLock.WaitAsync();

            try
            {
                Logger.Debug("Saving item '{Item}' to database...", item);
                await using var context = await _dbContextFactory.CreateDbContextAsync();

                if (item.Id == 0)
                {
                    // New item: insert into database
                    context.Items.Add(item);
                }
                else
                {
                    // Existing item: update database record
                    context.Items.Update(item);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error saving item '{Item}' to database", item);
            }
            finally
            {
                _databaseLock.Release();
            }
        }
    }
}