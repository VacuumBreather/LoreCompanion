namespace LoreCompanion.Models
{
    public interface ICharacterReferencing
    {
        int? CharacterId { get; set; }

        Character? Character { get; set; }
    }
}