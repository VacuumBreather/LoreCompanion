using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    [PublicAPI]
    public class Episode : EntityBase, IFilter, IComparable<Episode>
    {
        private Episode? _backup;
        private bool _inEdit;

        public int Number
        {
            get;
            set => Set(ref field, value);
        } = 1;

        [MaxLength(11)]
        public string VideoKey
        {
            get;
            set => Set(ref field, value);
        } = "";

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Episode {Number}";
        }

        public override void BeginEdit()
        {
            if (_inEdit)
            {
                return;
            }

            _backup = new Episode { Number = Number, VideoKey = VideoKey };
            _inEdit = true;
        }

        public override void CancelEdit()
        {
            if (!_inEdit)
            {
                return;
            }

            Number = _backup!.Number;
            VideoKey = _backup.VideoKey;
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
            return ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(Episode? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            return Number.CompareTo(other.Number);
        }
    }
}