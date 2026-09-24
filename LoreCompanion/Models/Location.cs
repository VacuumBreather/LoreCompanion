using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    [PublicAPI]
    public class Location : EntityBase, INamed, IEpisodeReferencing
    {
        private Location? _backup;
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

            _backup = new Location
            {
                Name = Name,
                ImageUrl = ImageUrl,
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
            ImageUrl = _backup!.ImageUrl;
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
    }
}