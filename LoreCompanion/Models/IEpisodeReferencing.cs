namespace LoreCompanion.Models
{
    public interface IEpisodeReferencing
    {
        int? EpisodeId { get; set; }

        Episode? Episode { get; set; }

        TimeSpan Timestamp { get; set; }
    }
}