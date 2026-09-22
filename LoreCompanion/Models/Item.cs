using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class Item : EntityBase
    {
        [MaxLength(128)]
        public string Name
        {
            get;
            set => Set(ref field, value);
        } = "";

        [MaxLength(2048)]
        public string Description
        {
            get;
            set => Set(ref field, value);
        } = "";

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }
}