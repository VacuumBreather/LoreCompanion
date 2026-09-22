using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    public abstract class EntityBase : INotifyPropertyChanged, IEditableObject, IEquatable<EntityBase>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Used by EF Core")]
        public int Id { get; set; }

        /// <summary>Notifies subscribers of the property change.</summary>
        /// <param name="propertyName">Name of the property.</param>
        [PublicAPI]
        public virtual void NotifyOfPropertyChange([CallerMemberName] string? propertyName = null)
        {
            Execute.OnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
        }

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

        /// <inheritdoc/>
        public abstract void BeginEdit();

        /// <inheritdoc/>
        public abstract void CancelEdit();

        /// <inheritdoc/>
        public abstract void EndEdit();

        protected virtual bool Set<T>(ref T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return false;
            }

            oldValue = newValue;

            NotifyOfPropertyChange(propertyName ?? string.Empty);

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}