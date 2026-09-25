using System.Diagnostics;
using Caliburn.Micro;
using JetBrains.Annotations;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels.Dialogs;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion.ViewModels
{
    public abstract class SectionScreen : Screen, IVideoPlayer
    {
        protected SectionScreen(string section, IDialogService dialogService)
        {
            DialogService = dialogService;
            DisplayName = GetType().Name.Replace("ViewModel", "");
            Section = section;
        }

        /// <inheritdoc/>
        public sealed override string DisplayName
        {
            get => base.DisplayName;
            set => base.DisplayName = value;
        }

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

            var videoUrl = string.Format(YouTubeHelper.VideoUrlFormatEmbeddedString, episode.VideoKey);
            var dialog = new VideoPlayerDialog(episode.ToString(), videoUrl);
            _ = DialogService.ShowDialogAsync(dialog);

            //Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
        }

        [UsedImplicitly]
        public void PlayVideo(IEpisodeReferencing? entity)
        {
            if (entity?.Episode is null)
            {
                return;
            }

            Logger.Debug("Playing video for {EntityName}: {Location}", entity.GetType().Name.ToLower(), entity);

            var videoUrl = string.Format(YouTubeHelper.VideoUrlFormatEmbeddedStringWithTime, entity.Episode.VideoKey, entity.Timestamp.TotalSeconds);
            var dialog = new VideoPlayerDialog(entity.Episode.ToString(), videoUrl);
            _ = DialogService.ShowDialogAsync(dialog);

            // var videoUrl = string.Format(
            //     YouTubeHelper.VideoUrlFormatStringWithTime,
            //     entity.Episode.VideoKey,
            //     entity.Timestamp.TotalSeconds);
            //
            // Process.Start(new ProcessStartInfo { FileName = videoUrl, UseShellExecute = true });
        }
    }
}