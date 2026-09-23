using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    [PublicAPI]
    public class Item : EntityBase, INamed
    {
        private Item? _backup;
        private bool _inEdit;

        [MaxLength(128)]
        public string Name
        {
            get;
            set => Set(ref field, value);
        } = "";

        [MaxLength(512)]
        public string ImageUrl
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

        public ItemType Type
        {
            get;
            set => Set(ref field, value);
        }

        [MaxLength(256)]
        public string Location
        {
            get;
            set => Set(ref field, value);
        } = "";

        public Episode? Episode
        {
            get;
            set => Set(ref field, value);
        }

        public TimeSpan Timestamp
        {
            get;
            set => Set(ref field, value);
        }

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

            _backup = new Item
            {
                Name = Name,
                ImageUrl = ImageUrl,
                Description = Description,
                Type = Type,
                Location = Location,
                Episode = Episode,
                Timestamp = Timestamp,
            };

            _inEdit = true;
        }

        public override void CancelEdit()
        {
            if (!_inEdit)
            {
                return;
            }

            Name = _backup!.Name;
            ImageUrl = _backup.ImageUrl;
            Description = _backup.Description;
            Type = _backup.Type;
            Location = _backup.Location;
            Episode = _backup.Episode;
            Timestamp = _backup.Timestamp;
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