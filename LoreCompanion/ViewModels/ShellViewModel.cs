using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Data;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Extensions;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using LoreCompanion.Views.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using R3;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public sealed class ShellViewModel : Conductor<SectionScreen>.Collection.OneActive
    {
        private readonly IDbContextFactory<LoreDbContext> _dbContextFactory;
        private readonly CachedDataLoader _cachedDataLoader;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly IEventAggregator _eventAggregator;
        private readonly SectionScreen _dashboard;

        private CancellationTokenSource? _databaseUpdate;
        private int _busyCount;

        public ShellViewModel(
            IEnumerable<SectionScreen> sections,
            IDbContextFactory<LoreDbContext> dbContextFactory,
            CachedDataLoader cachedDataLoader,
            IDialogService dialogService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
        {
            _dbContextFactory = dbContextFactory;
            _cachedDataLoader = cachedDataLoader;
            _dialogService = dialogService;
            _notificationService = notificationService;
            _eventAggregator = eventAggregator;

            ItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            ItemsView.GroupDescriptions!.Add(new PropertyGroupDescription(nameof(SectionScreen.Section)));

            ItemsView.CustomSort = Comparer<SectionScreen>.Create((a, b) =>
            {
                var result = NavigationSection.Order.IndexOf(a.Section)
                                              .CompareTo(NavigationSection.Order.IndexOf(b.Section));

                if (result != 0)
                {
                    return result;
                }

                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            Items.AddRange(sections);

            var firstGroup = (CollectionViewGroup)ItemsView.Groups!.First();
            _dashboard = (SectionScreen)firstGroup.Items.First();
        }

        public Version CurrentDatabaseVersion
        {
            get;
            private set => Set(ref field, value);
        } = Version.Parse("0.0.0");

        public ListCollectionView ItemsView { get; }

        public bool IsBusy => _busyCount > 0;

        public DatabaseStatus DatabaseStatus
        {
            get;
            private set => Set(ref field, value);
        } = DatabaseStatus.Unknown;

        private static ILogger Logger { get; } = LogManager.GetLogger();

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new())
        {
            Logger.Information("Closing application...");

            try
            {
                await _cachedDataLoader.DisposeAsync();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error closing cached data loader");
            }

            if (_databaseUpdate is not null)
            {
                try
                {
                    await _databaseUpdate.CancelAsync();
                    _databaseUpdate = null;
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error awaiting for database update");
                }
            }

            try
            {
                if (_notificationService is IDeactivate deactivateNotifications)
                {
                    Logger.Debug("Closing notification service...");
                    await deactivateNotifications.DeactivateAsync(true, cancellationToken);
                }

                if (_dialogService is IDeactivate deactivateDialogs)
                {
                    Logger.Debug("Closing dialog service...");
                    await deactivateDialogs.DeactivateAsync(true, cancellationToken);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error closing dialogs and notifications");
            }

            return await base.CanCloseAsync(cancellationToken);
        }

        [PublicAPI]
        public async Task PublishDatabaseAsync()
        {
            if (!AppHelper.IsAdminMode)
            {
                Log.Error("Cannot publish database in non-admin mode");

                return;
            }

            var result = await _dialogService.ShowQueryDialogAsync(
                             "Publish Database",
                             "Are you sure you want to publish a new database version?",
                             DialogResults.YesNo,
                             DialogResult.Yes);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var dbContext = await _dbContextFactory.CreateDbContextAsync();

                var currentVersion =
                    dbContext.DatabaseReleases.AsEnumerable()
                             .OrderByDescending(r => r.PublishedAt)
                             .Select(r => r.Version)
                             .FirstOrDefault() ??
                    Version.Parse("0.0.0");

                Version newVersion = new(currentVersion.Major, currentVersion.Minor + 1, currentVersion.Build);

                var releaseNotesDialog = new ReleaseNotesDialog(newVersion);
                _ = await _dialogService.ShowDialogAsync(releaseNotesDialog);

                dbContext.DatabaseReleases.Add(
                    new DatabaseRelease
                    {
                        Version = newVersion,
                        PublishedAt = DateTime.UtcNow,
                        ReleaseNotes = releaseNotesDialog.ReleaseNotes,
                    });

                await dbContext.SaveChangesAsync();

                CurrentDatabaseVersion = newVersion;

                _ = _notificationService.ShowNotificationAsync(
                    "Database",
                    $"Version {newVersion} released",
                    NotificationType.Success);
            }
            catch (Exception e)
            {
                _ = _notificationService.ShowNotificationAsync(
                    "Database Error",
                    $"Unable to create database release\n{e.Message}",
                    NotificationType.Error);

                Logger.Error(e, "Unable to create database release");
            }
        }

        [PublicAPI]
        public async Task RefreshDatabaseAsync()
        {
            try
            {
                using var scope = SetBusy();

                _databaseUpdate = new CancellationTokenSource();

                await DeactivateItemAsync(ActiveItem, false, _databaseUpdate.Token);
                await UpdateDatabase(CurrentDatabaseVersion, _databaseUpdate.Token);

                Logger.Information("Showing dashboard...");

                await ActivateItemAsync(_dashboard, _databaseUpdate.Token);
            }
            catch (OperationCanceledException) when (_databaseUpdate is { IsCancellationRequested: true })
            {
                // Ignore and proceed
                Logger.Debug("Database update was canceled");
            }
            finally
            {
                _databaseUpdate?.Dispose();
                _databaseUpdate = null;
            }
        }

        protected override async Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Application initialized");

            if (AppHelper.IsAdminMode)
            {
                Logger.Information("Admin mode detected!");
            }

            CurrentDatabaseVersion = await MigrateDatabaseAsync(cancellationToken);
            Logger.Information("Database version: {Version}", CurrentDatabaseVersion);

            if (_notificationService is IActivate activateNotifications)
            {
                Logger.Debug("Activating notification service...");
                await activateNotifications.ActivateAsync(cancellationToken);
            }

            if (_dialogService is IActivate activateDialogs)
            {
                Logger.Debug("Activating dialog service...");
                await activateDialogs.ActivateAsync(cancellationToken);
            }

            _ = RefreshDatabaseAsync();
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

        private async Task UpdateDatabase(Version currentVersion, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.Configure();

            DatabaseManifest? manifest;
            DatabaseStatus = DatabaseStatus.Unknown;

            try
            {
                await using var scope = await _dialogService.ShowBusyDialogAsync(
                                            "Please wait",
                                            "Checking for database update...",
                                            cancellationToken);

                var json = await client.GetStringAsync(AppHelper.DatabaseManifestUrl, cancellationToken);
                manifest = JsonSerializer.Deserialize<DatabaseManifest>(json);
            }
            catch (HttpRequestException e)
            {
                Logger.Error(e, "Failed to retrieve database manifest");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Failed to retrieve database manifest.\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }
            catch (OperationCanceledException e) when (cancellationToken.IsCancellationRequested)
            {
                Logger.Warning(e, "Database manifest retrieval canceled");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database manifest retrieval canceled.\n{e.Message}",
                    NotificationType.Warning,
                    cancellationToken: CancellationToken.None);

                return;
            }
            catch (OperationCanceledException e)
            {
                Logger.Error(e, "Database manifest retrieval timed out");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database manifest retrieval timed out.\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }
            catch (JsonException e)
            {
                Logger.Error(e, "Failed to parse database manifest");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Failed to parse database manifest.\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }

            if (manifest?.Version is null or { Major: 0, Minor: 0 })
            {
                Logger.Error("Database manifest does not contain a valid version");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    "Database manifest does not contain a valid version.",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }

            if (manifest.Version <= currentVersion)
            {
                DatabaseStatus = DatabaseStatus.UpToDate;

                return;
            }

            DatabaseStatus = DatabaseStatus.OutOfDate;

            var result = await _dialogService.ShowQueryDialogAsync(
                             "Database update",
                             $"New database update available (v{manifest.Version})\nDo you want to update now?",
                             DialogResults.YesNo,
                             DialogResult.Yes,
                             cancellationToken);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (manifest.DownloadUrl is null)
            {
                Logger.Error("Database manifest does not contain a download URL");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    "Database manifest does not contain a download URL.",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }

            if (string.IsNullOrWhiteSpace(manifest.Sha256))
            {
                Logger.Error("Database manifest does not contain a SHA-256 hash");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    "Database manifest does not contain a SHA-256 hash.",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }

            byte[] bytes;

            try
            {
                await using var scope = await _dialogService.ShowBusyDialogAsync(
                                            "Please wait",
                                            "Downloading database update...",
                                            cancellationToken);

                bytes = await client.GetByteArrayAsync(manifest.DownloadUrl, cancellationToken);
            }
            catch (HttpRequestException e)
            {
                Logger.Error(e, "Failed to download database {Version}", manifest.Version);

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Failed to download database {manifest.Version}\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }
            catch (OperationCanceledException e) when (cancellationToken.IsCancellationRequested)
            {
                Logger.Warning(e, "Database {Version} download canceled", manifest.Version);

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database {manifest.Version} download canceled\n{e.Message}",
                    NotificationType.Warning,
                    cancellationToken: CancellationToken.None);

                return;
            }
            catch (OperationCanceledException e)
            {
                Logger.Error(e, "Database {Version} download timed out", manifest.Version);

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database {manifest.Version} download timed out\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }

            var hash = SHA256.HashData(bytes);
            var hashString = Convert.ToHexString(hash);

            if (!string.Equals(hashString, manifest.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Error("Database {Version} failed SHA-256 verification", manifest.Version);

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database {manifest.Version} failed SHA-256 verification",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);

                return;
            }

            var tempPath = $"{AppHelper.DatabasePath}.tmp";

            try
            {
                await using var scope = await _dialogService.ShowBusyDialogAsync(
                                            "Please wait",
                                            "Updating database...",
                                            cancellationToken);

                SqliteConnection.ClearAllPools();

                await File.WriteAllBytesAsync(tempPath, bytes, cancellationToken);

                File.Move(tempPath, AppHelper.DatabasePath, true);

                CurrentDatabaseVersion = await MigrateDatabaseAsync(cancellationToken);
                DatabaseStatus = DatabaseStatus.UpToDate;
                await _eventAggregator.PublishOnUIThreadAsync(new DatabaseUpdatedEvent(), cancellationToken);

                Logger.Information("Database updated to version {Version}", manifest.Version);

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database updated to version {manifest.Version}",
                    NotificationType.Success,
                    cancellationToken: CancellationToken.None);
            }
            catch (OperationCanceledException e)
            {
                Logger.Warning(e, "Database update was canceled");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Database update was canceled\n{e.Message}",
                    NotificationType.Warning,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to update database");

                _ = _notificationService.ShowNotificationAsync(
                    "Database update",
                    $"Failed to update database\n{e.Message}",
                    NotificationType.Error,
                    cancellationToken: CancellationToken.None);
            }
            finally
            {
                // Don't leave a partially downloaded database behind.
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (Exception e)
                    {
                        // Nothing useful to do if cleanup itself fails.
                        Logger.Warning(e, "Failed to delete temporary database file");

                        _ = _notificationService.ShowNotificationAsync(
                            "Database update",
                            $"Failed to delete temporary database file\n{e.Message}",
                            NotificationType.Warning,
                            cancellationToken: CancellationToken.None);
                    }
                }
            }
        }

        private async Task<Version> MigrateDatabaseAsync(CancellationToken cancellationToken)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await DatabaseHelper.ClearStaleMigrationLockAsync(context, cancellationToken);

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));
                await context.Database.MigrateAsync(cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException(
                    "Database migration timed out — the migration lock may be stuck. " +
                    "Check for another running instance or a stale __EFMigrationsLock row.");
            }

            var version = context.DatabaseReleases.AsEnumerable()
                                 .Select(r => r.Version)
                                 .OrderDescending()
                                 .FirstOrDefault();

            await context.Database.CloseConnectionAsync();

            return version ?? Version.Parse("0.0.0");
        }
    }
}