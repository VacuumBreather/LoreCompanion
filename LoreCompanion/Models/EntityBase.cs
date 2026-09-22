using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    public abstract class EntityBase
    {
        public int Id { get; set; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not EntityBase other || (GetType() != other.GetType()))
            {
                return false;
            }

            return (Id != 0) && (Id == other.Id);
        }

        [SuppressMessage(
            "ReSharper",
            "NonReadonlyMemberInGetHashCode",
            Justification = "Id is not modified after entity is persisted")]
        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), Id);
        }
    }
}