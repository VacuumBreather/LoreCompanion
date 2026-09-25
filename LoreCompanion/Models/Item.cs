using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    [PublicAPI]
    public class Item : EntityBase, INamed, IEpisodeReferencing, ILocationReferencing, IFilter, IComparable<Item>
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

        [ForeignKey(nameof(Location))]
        public int? LocationId
        {
            get;
            set
            {
                if (!Set(ref field, value))
                {
                    return;
                }

                if (Location?.Id != value)
                {
                    Location = null;
                }
            }
        }

        public Location? Location
        {
            get;
            set => Set(ref field, value);
        }

        [ForeignKey(nameof(Episode))]
        public int? EpisodeId
        {
            get;
            set
            {
                if (!Set(ref field, value))
                {
                    return;
                }

                if (Episode?.Id != value)
                {
                    Episode = null;
                }
            }
        }

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
            return Name;
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
                LocationId = LocationId,
                Location = Location,
                EpisodeId = EpisodeId,
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
            LocationId = _backup.LocationId;
            Location = _backup.Location;
            EpisodeId = _backup.EpisodeId;
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

        public bool Filter(string searchText)
        {
            return Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   Description.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(Item? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            var typeComparison = Type.CompareTo(other.Type);

            if (typeComparison != 0)
            {
                return typeComparison;
            }

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}