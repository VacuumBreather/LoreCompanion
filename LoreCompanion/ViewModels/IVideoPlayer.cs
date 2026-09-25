using LoreCompanion.Models;

namespace LoreCompanion.ViewModels
{
    public interface IVideoPlayer
    {
        void PlayEpisodeVideo(Episode episode);

        void PlayVideo(IEpisodeReferencing entity);
    }
}