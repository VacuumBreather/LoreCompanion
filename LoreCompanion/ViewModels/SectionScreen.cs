using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Extensions;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public abstract class SectionScreen : Screen, IVideoPlayer
    {
        protected SectionScreen(string section, IDialogService dialogService, bool canEdit = true)
        {
            DialogService = dialogService;
            DisplayName = GetType().Name.Replace("ViewModel", "");
            Section = section;
            CanEdit = canEdit && AppHelper.IsAdminMode;
        }

        /// <inheritdoc/>
        public sealed override string DisplayName
        {
            get => base.DisplayName;
            set => base.DisplayName = value;
        }

        [UsedImplicitly]
        public bool CanEdit { get; }

        public string Section { get; }

        protected IDialogService DialogService { get; }

        protected ILogger Logger => field ??= LogManager.GetLogger(GetType());

        [UsedImplicitly]
        public void PlayEpisodeVideo(Episode? episode)
        {
            if (episode is null)
            {
                return;
            }

            Logger.Debug("Playing video for episode '{EpisodeName}'", episode.ToString());

            var dialog = new VideoPlayerDialog(episode.ToString(), episode.VideoKey);
            _ = DialogService.ShowDialogAsync(dialog);
        }

        [UsedImplicitly]
        public void PlayVideo(IEpisodeReferencing? entity)
        {
            if (entity?.Episode is null)
            {
                return;
            }

            Logger.Debug("Playing video for {EntityName}: {Entity}", entity.GetType().Name.ToLower(), entity);

            var dialog = new VideoPlayerDialog(entity.GetVideoTitle(), entity.Episode.VideoKey, entity.Timestamp);
            _ = DialogService.ShowDialogAsync(dialog);
        }
    }
}