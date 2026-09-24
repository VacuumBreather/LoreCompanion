using LoreCompanion.Models;

namespace LoreCompanion.ViewModels
{
    public class SaveEntityEventArgs<TEntity> : EventArgs
        where TEntity : EntityBase
    {
        public required TEntity Entity { get; init; }
    }
}