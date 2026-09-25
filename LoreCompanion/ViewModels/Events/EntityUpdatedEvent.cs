using LoreCompanion.Models;

namespace LoreCompanion.ViewModels.Events
{
    public readonly struct EntityUpdatedEvent<TEntity>
        where TEntity : EntityBase
    {
    }
}