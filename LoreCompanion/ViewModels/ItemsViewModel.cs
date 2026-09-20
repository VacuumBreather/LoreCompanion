using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Models;
using LoreCompanion.Views;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    public class ItemsViewModel : SectionScreen
    {
        private readonly IDbContextFactory<LoreDbContext> _dbContextFactory;

        public ItemsViewModel(IDbContextFactory<LoreDbContext> dbContextFactory) : base(NavigationSection.Lore)
        {
            _dbContextFactory = dbContextFactory;
            DisplayName = "Items";
        }

        protected override async Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = OnFilter;

            Items.Clear();
            Items.AddRange(context.Items);

            SelectedItem = Items.FirstOrDefault();
        }

        private bool OnFilter(object obj)
        {
            var item = (Item)obj;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            return item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                   item.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        public ICollectionView ItemsView { get; private set; }

        public Task CreateNewAsync()
        {
            var newItem = new Item();
            newItem.Name = "Demo";
            newItem.Description = "Demo Description";
            Items.Add(newItem);
            SelectedItem = newItem;

            // Immediately switch to editable mode for the new item
            EditMode = EditMode.Editable;

            return Task.CompletedTask;
        }

        public string SearchText
        {
            get;
            set
            {
                if (Set(ref field, value))
                {
                    ItemsView.Refresh();
                }
            }
        }

        public async Task DeleteCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            if (SelectedItem.Id == 0)
            {
                // New item: no action required
            }
            else
            {
                // Existing item: update database record
                context.Items.Remove(SelectedItem);
            }

            var oldIndex = Items.IndexOf(SelectedItem);
            Items.Remove(SelectedItem);

            Item? newSelectedItem = null;

            if (Items.Count > oldIndex)
            {
                newSelectedItem = Items[oldIndex];
            }
            else if (oldIndex > 0 && Items.Count > oldIndex - 1)
            {

                newSelectedItem = Items[oldIndex - 1];
            }

            SelectedItem = newSelectedItem;

            await context.SaveChangesAsync();
        }

        public bool CanDeleteCurrent => SelectedItem is not null;

        public EditMode EditMode
        {
            get;
            set
            {
                if (Set(ref field, value))
                {
                    NotifyOfPropertyChange(nameof(CanEditCurrent));
                    NotifyOfPropertyChange(nameof(CanSaveCurrent));
                }
            }
        } = EditMode.ReadOnly;

        public bool CanEditCurrent => SelectedItem is not null && EditMode == EditMode.ReadOnly;

        public void EditCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            EditMode = EditMode.Editable;
        }

        public bool CanSaveCurrent => SelectedItem is not null && EditMode == EditMode.Editable;

        public async Task SaveCurrentAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            await SaveSelectedItemAsync();
            EditMode = EditMode.ReadOnly;
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (EditMode == EditMode.Editable)
            {
                return SaveSelectedItemAsync();
            }

            return base.OnDeactivateAsync(close, cancellationToken);
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

        public BindableCollection<Item> Items { get; set; } = new();

        public Item? SelectedItem
        {
            get;
            set
            {
                if (Set(ref field, value))
                {
                    NotifyOfPropertyChange(nameof(CanDeleteCurrent));
                    NotifyOfPropertyChange(nameof(CanEditCurrent));
                    NotifyOfPropertyChange(nameof(CanSaveCurrent));

                    if (EditMode == EditMode.Editable)
                    {
                        SaveCurrentAsync().GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}