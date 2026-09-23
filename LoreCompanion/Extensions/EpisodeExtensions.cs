using LoreCompanion.Models;

namespace LoreCompanion.Extensions
{
    public static class EpisodeExtensions
    {
        public static int GetEpisodeNumber(this Episode episode)
        {
            var episodeNumberString = episode.Name.Replace("Episode", "").Trim();

            return int.TryParse(episodeNumberString, out var number) ? number : 0;
        }
    }
}