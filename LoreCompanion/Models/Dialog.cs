using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    [PublicAPI]
    public class Dialog : EntityBase,
                          ICharacterReferencing,
                          IEpisodeReferencing,
                          ILocationReferencing,
                          IComparable<Dialog>
    {
        private Dialog? _backup;
        private bool _inEdit;

        [MaxLength(128)]
        public string Context
        {
            get;
            set => Set(ref field, value);
        } = "";

        [MaxLength(2048)]
        public string Content
        {
            get;
            set => Set(ref field, value);
        } = "";

        [ForeignKey(nameof(Character))]
        public int? CharacterId
        {
            get;
            set
            {
                if (!Set(ref field, value))
                {
                    return;
                }

                if (Character?.Id != value)
                {
                    Character = null;
                }
            }
        }

        public Character? Character
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
            return $"{Character?.Name ?? "Unknown"} - {Context}";
        }

        public override void BeginEdit()
        {
            if (_inEdit)
            {
                return;
            }

            _backup = new Dialog
            {
                Context = Context,
                Content = Content,
                CharacterId = CharacterId,
                Character = Character,
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

            Context = _backup!.Context;
            Content = _backup.Content;
            CharacterId = _backup.CharacterId;
            Character = _backup.Character;
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

        public int CompareTo(Dialog? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            var result = Character?.CompareTo(other.Character) ?? -1;

            if (result != 0)
            {
                return result;
            }

            result = Episode?.CompareTo(other.Episode) ?? -1;

            if (result != 0)
            {
                return result;
            }

            result = TimeSpan.Compare(Timestamp, other.Timestamp);

            if (result != 0)
            {
                return result;
            }

            return string.Compare(Context, other.Context, StringComparison.OrdinalIgnoreCase);
        }
    }
}