namespace LoreCompanion.Models
{
    public interface IEpisodeReferencing
    {
        Episode? Episode { get; set; }

        TimeSpan Timestamp { get; set; }
    }
}