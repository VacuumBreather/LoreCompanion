using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    public abstract class EntityBase : IEquatable<EntityBase>
    {
        public int Id { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityBase);
        }

        [SuppressMessage(
            "ReSharper",
            "NonReadonlyMemberInGetHashCode",
            Justification = "Id is not modified after entity is persisted")]
        public override int GetHashCode()
        {
            return Id;
        }

        /// <inheritdoc/>
        public bool Equals(EntityBase? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return (Id != 0) && (Id == other.Id);
        }
    }
}