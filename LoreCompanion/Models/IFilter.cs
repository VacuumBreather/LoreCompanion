namespace LoreCompanion.Models
{
    public interface IFilter
    {
        bool Filter(string searchText);
    }
}