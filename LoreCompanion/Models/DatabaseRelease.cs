using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class DatabaseRelease
    {
        public int Id { get; set; }

        public Version Version { get; set; } = new();

        public DateTimeOffset PublishedAt { get; set; }

        [MaxLength(2048)]
        public string? ReleaseNotes { get; set; }
    }
}