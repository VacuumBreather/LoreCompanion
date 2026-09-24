namespace LoreCompanion.Models
{
    public interface ILocationReferencing
    {
        int? LocationId { get; set; }

        Location? Location { get; set; }
    }
}