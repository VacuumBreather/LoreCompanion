using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class Item
    {
        public int Id { get; set; }

        [MaxLength(128)]
        public string Name { get; set; } = "";

        [MaxLength(2048)]
        public string Description { get; set; } = "";

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }
}