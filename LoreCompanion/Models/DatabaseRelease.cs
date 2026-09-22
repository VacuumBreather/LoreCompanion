using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class DatabaseRelease : EntityBase
    {
        public Version Version
        {
            get;
            set => Set(ref field, value);
        } = new();

        public DateTimeOffset PublishedAt
        {
            get;
            set => Set(ref field, value);
        }

        [MaxLength(2048)]
        public string? ReleaseNotes
        {
            get;
            set => Set(ref field, value);
        }

        public override void BeginEdit()
        {
            throw new NotSupportedException();
        }

        public override void CancelEdit()
        {
            throw new NotSupportedException();
        }

        public override void EndEdit()
        {
            throw new NotSupportedException();
        }
    }
}