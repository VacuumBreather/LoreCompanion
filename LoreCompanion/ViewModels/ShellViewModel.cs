using System.Windows.Data;
using Caliburn.Micro;
using LoreCompanion.Extensions;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;
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

        public ShellViewModel(
            IEnumerable<SectionScreen> sections,
            IDbContextFactory<LoreDbContext> dbContextFactory,
            CachedDataLoader cachedDataLoader,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _dbContextFactory = dbContextFactory;
            _cachedDataLoader = cachedDataLoader;
            _dialogService = dialogService;
            _notificationService = notificationService;

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
        }

        public Version CurrentDatabaseVersion
        {
            get;
            private set => Set(ref field, value);
        } = Version.Parse("0.0.0");

        public Version CurrentApplicationVersion { get; private set; } =
            typeof(ShellViewModel).Assembly.GetName().Version ?? Version.Parse("0.0.0");

        public ListCollectionView ItemsView { get; }

        private static ILogger Logger { get; } = LogManager.GetLogger();

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new())
        {
            Logger.Information("Closing application...");

            await using var scope = await _dialogService.ShowBusyDialogAsync(
                                        "Please Wait",
                                        "Closing application...",
                                        cancellationToken);

            await _cachedDataLoader.DisposeAsync();

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

            return await base.CanCloseAsync(cancellationToken);
        }

        public async Task RefreshDatabaseAsync()
        {
        }

        public async Task PublishDatabaseAsync()
        {
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

        protected override async Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Application initialized");

            if (AppHelper.IsAdminMode)
            {
                Logger.Information("Admin mode detected!");
            }

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

            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken))
            {
                CurrentDatabaseVersion = dbContext.DatabaseReleases.AsEnumerable()
                                                  .OrderByDescending(r => r.PublishedAt)
                                                  .Select(r => r.Version)
                                                  .FirstOrDefault() ??
                                         Version.Parse("0.0.0");
            }

            Logger.Information("Showing dashboard...");

            var firstGroup = (CollectionViewGroup)ItemsView.Groups!.First();
            var firstScreen = (SectionScreen)firstGroup.Items.First();

            await ActivateItemAsync(firstScreen, cancellationToken);
        }
    }
}