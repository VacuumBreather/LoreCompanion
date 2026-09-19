using System.Collections.ObjectModel;
using Caliburn.Micro;
using LoreCompanion.Models;
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

            Items.Clear();
            Items.AddRange(context.Items);

            SelectedItem = Items.FirstOrDefault();
        }

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
            set => Set(ref field, value);
        } = EditMode.ReadOnly;

        public bool CanToggleEdit => SelectedItem is not null;

        public async Task ToggleEditAsync()
        {
            if (EditMode == EditMode.Editable)
            {
                await SaveSelectedItemAsync();
                EditMode = EditMode.ReadOnly;
            }
            else
            {
                EditMode = EditMode.Editable;
            }
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
                    NotifyOfPropertyChange(nameof(CanToggleEdit));
                    if (EditMode == EditMode.Editable)
                    {
                        ToggleEditAsync().GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}