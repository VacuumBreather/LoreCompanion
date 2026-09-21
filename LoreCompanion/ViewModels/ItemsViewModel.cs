using System.ComponentModel;
using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using Microsoft.EntityFrameworkCore;
using R3;

namespace LoreCompanion.ViewModels
{
    public sealed class ItemsViewModel : SectionScreen
    {
        private readonly IDbContextFactory<LoreDbContext> _dbContextFactory;
        private readonly IDialogService _dialogService;
        private IDisposable? _subscription;

        private int _busyCount;

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

        public BindableCollection<Item> Items { get; } = [];

        public Item? SelectedItem
        {
            get;
            set
            {
                if (!Set(ref field, value))
                {
                    return;
                }

                NotifyOfPropertyChange(nameof(CanEditCurrent));
                NotifyOfPropertyChange(nameof(CanSaveCurrent));

                if (EditMode == EditMode.Editable)
                {
                    SaveCurrentAsync().GetAwaiter().GetResult();
                }
            }
        }

        public bool IsBusy => _busyCount > 0;

        public Task CreateNewAsync()
        {
            var newItem = new Item { Name = "Demo", Description = "Demo Description" };
            Items.Add(newItem);
            SelectedItem = newItem;

            // Immediately switch to editable mode for the new item
            EditMode = EditMode.Editable;

            return Task.CompletedTask;
        }

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

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            if (item.Id == 0)
            {
                // New item: no action required
            }
            else
            {
                // Existing item: update database record
                context.Items.Remove(item);
            }

            var oldIndex = Items.IndexOf(item);
            var wasSelectedItem = SelectedItem?.Id == item.Id;

            Items.Remove(item);

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

            await context.SaveChangesAsync();
        }

        public void EditCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            EditMode = EditMode.Editable;
        }

        public async Task SaveCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            await SaveSelectedItemAsync();
            EditMode = EditMode.ReadOnly;
        }

        public IDisposable SetBusy()
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

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            _subscription = this.ObservePropertyChanged(x => x.SearchText)
                                .Debounce(TimeSpan.FromMilliseconds(250))
                                .ObserveOnCurrentDispatcher()
                                .Subscribe(_ => ItemsView.Refresh());

            // Kick off progressive background loading without blocking view display
            _ = LoadItemsProgressivelyAsync(cancellationToken);

            return Task.CompletedTask;

            // await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            //
            // var items = await context.Items.AsNoTracking().ToListAsync(cancellationToken);
            //
            // Items.Clear();
            // Items.AddRange(items);
            //
            // SelectedItem = Items.FirstOrDefault();
        }

        private async Task LoadItemsProgressivelyAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();

            try
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

                Items.Clear();

                await Task.Delay(5000);

                // Stream items asynchronously from the database
                await foreach (var item in context.Items.AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
                {
                    Execute.OnUIThread(() =>
                    {
                        Items.Add(item);

                        // Automatically select the first item once it arrives
                        if (SelectedItem is null)
                        {
                            SelectedItem = item;
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when navigating away; terminate stream cleanly
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                _subscription?.Dispose();
            }

            if (EditMode == EditMode.Editable)
            {
                return SaveSelectedItemAsync();
            }

            return base.OnDeactivateAsync(close, cancellationToken);
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

        private async Task SaveSelectedItemAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            if (SelectedItem.Id == 0)
            {
                // New item: insert into database
                context.Items.Add(SelectedItem);
            }
            else
            {
                // Existing item: update database record
                context.Items.Update(SelectedItem);
            }

            await context.SaveChangesAsync();
        }
    }
}