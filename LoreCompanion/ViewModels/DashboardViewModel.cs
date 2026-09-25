using System.Runtime.CompilerServices;
using System.Windows.Data;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Attributes;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Events;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.ViewModels
{
    [Dashboard, UsedImplicitly]
    public class DashboardViewModel : SectionScreen,
                                      IHandle<DatabaseUpdatedEvent>,
                                      IHandle<EntityUpdatedEvent<Location>>,
                                      IHandle<EntityUpdatedEvent<Character>>,
                                      IHandle<EntityUpdatedEvent<Item>>,
                                      IHandle<EntityUpdatedEvent<Dialog>>
    {
        private readonly IDbContextFactory<LoreDbContext> _dbContextFactory;
        private readonly INotificationService _notificationService;

        private bool _databaseRefreshNeeded = true;
        private int _busyCount;
        private Task? _loadingTask;
        private CancellationTokenSource? _loadingCts;

        public DashboardViewModel(
            IDbContextFactory<LoreDbContext> dbContextFactory,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
            : base(NavigationSection.Overview, dialogService)
        {
            _dbContextFactory = dbContextFactory;
            _notificationService = notificationService;
            eventAggregator.SubscribeOnPublishedThread(this);

            ItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            ItemsView.CustomSort = Comparer<EntityBase>.Create(CompareEntities);
        }

        [UsedImplicitly]
        public BindableCollection<EntityBase> Items { get; } = [];

        [UsedImplicitly]
        public ListCollectionView ItemsView { get; }

        public bool IsBusy => _busyCount > 0;

        private SemaphoreSlim DatabaseLock { get; } = new(1, 1);

        public Task HandleAsync(DatabaseUpdatedEvent message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdatedEvent<Location> message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdatedEvent<Character> message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdatedEvent<Item> message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdatedEvent<Dialog> message, CancellationToken cancellationToken)
        {
            _databaseRefreshNeeded = true;

            return Task.CompletedTask;
        }

        protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            Logger.Debug("Activated");

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

        private static async IAsyncEnumerable<EntityBase> GetAllEntitiesAsync(
            LoreDbContext context,
            int episodeId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var loc in context.Locations.Where(l => l.EpisodeId == episodeId)
                                             .AsNoTracking()
                                             .Include(loc => loc.Episode)
                                             .AsAsyncEnumerable()
                                             .WithCancellation(cancellationToken))
            {
                yield return loc;
            }

            await foreach (var character in context.Characters.Where(l => l.EpisodeId == episodeId)
                                                   .AsNoTracking()
                                                   .Include(c => c.Episode)
                                                   .Include(c => c.Location)
                                                   .AsAsyncEnumerable()
                                                   .WithCancellation(cancellationToken))
            {
                yield return character;
            }

            await foreach (var item in context.Items.Where(l => l.EpisodeId == episodeId)
                                              .AsNoTracking()
                                              .Include(it => it.Episode)
                                              .Include(it => it.Location)
                                              .AsAsyncEnumerable()
                                              .WithCancellation(cancellationToken))
            {
                yield return item;
            }

            await foreach (var dialog in context.Dialogs.Where(l => l.EpisodeId == episodeId)
                                                .AsNoTracking()
                                                .Include(d => d.Episode)
                                                .Include(d => d.Location)
                                                .Include(d => d.Character)
                                                .AsAsyncEnumerable()
                                                .WithCancellation(cancellationToken))
            {
                yield return dialog;
            }
        }

        private int CompareEntities(EntityBase x, EntityBase y)
        {
            if (x is not IEpisodeReferencing hasEpisodeX || y is not IEpisodeReferencing hasEpisodeY)
            {
                return 0;
            }

            return -hasEpisodeX.Timestamp.CompareTo(hasEpisodeY.Timestamp);
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

        private async Task LoadEntitiesAsync(CancellationToken cancellationToken)
        {
            using var busy = SetBusy();

            await Task.Yield();
            var lockAcquired = false;

            try
            {
                Logger.Debug("Loading items...");

                await DatabaseLock.WaitAsync(cancellationToken);
                lockAcquired = true;

                if (_databaseRefreshNeeded)
                {
                    await LoadEntitiesInternalAsync(cancellationToken);

                    _databaseRefreshNeeded = false;
                }
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
                if (lockAcquired)
                {
                    DatabaseLock.Release();
                }
            }
        }

        private async Task LoadEntitiesInternalAsync(CancellationToken cancellationToken)
        {
            Execute.OnUIThread(() => { Items.Clear(); });

            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var episode = context.Episodes.OrderByDescending(e => e.Number).FirstOrDefault();

            if (episode is null)
            {
                return;
            }

            const int BatchSize = 25;
            var buffer = new List<EntityBase>(BatchSize);

            // Stream all entities sequentially through the unified async enumerable
            await foreach (var entity in GetAllEntitiesAsync(context, episode.Id, cancellationToken))
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

            Logger.Debug("All entities loaded");
        }

        private void UpdateItems(List<EntityBase> buffer)
        {
            var chunk = buffer.ToArray();
            buffer.Clear();

            Execute.OnUIThread(() => { Items.AddRange(chunk); });
        }
    }
}