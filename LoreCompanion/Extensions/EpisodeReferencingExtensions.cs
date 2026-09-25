using LoreCompanion.Models;

namespace LoreCompanion.Extensions
{
    public static class EpisodeReferencingExtensions
    {
        public static string GetVideoTitle(this IEpisodeReferencing entity)
        {
            return entity switch
            {
                Location location => location.Name,
                Item item => item.Name,
                Dialog dialog =>
                    $"{dialog.Character?.Name ?? "Unknown"} - {(string.IsNullOrEmpty(dialog.Context) ? "Unknown" : dialog.Context)}",
                Character character => character.Name,
                var _ => "",
            };
        }
    }
}