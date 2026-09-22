using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LoreCompanion.Models
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class Item : EntityBase
    {
        private Item? _backup;
        private bool _inEdit;

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

        public override void BeginEdit()
        {
            if (_inEdit)
            {
                return;
            }

            _backup = new Item { Name = Name, Description = Description };
            _inEdit = true;
        }

        public override void CancelEdit()
        {
            if (!_inEdit)
            {
                return;
            }

            Name = _backup!.Name;
            Description = _backup.Description;
            _backup = null;
            _inEdit = false;
        }

        public override void EndEdit()
        {
            if (!_inEdit)
            {
                return;
            }

            _backup = null;
            _inEdit = false;
        }
    }
}