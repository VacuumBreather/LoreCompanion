using LoreCompanion.Models;

namespace LoreCompanion.ViewModels
{
    public interface IVideoPlayer
    {
        void PlayVideo(Episode episode);

        void PlayVideo(IEpisodeReferencing entity);
    }
}