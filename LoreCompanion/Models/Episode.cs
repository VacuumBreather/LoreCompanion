using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace LoreCompanion.Models
{
    [PublicAPI]
    public class Episode : EntityBase, INamed
    {
        private Episode? _backup;
        private bool _inEdit;

        [MaxLength(128)]
        public string Name
        {
            get;
            set => Set(ref field, value);
        } = "";

        [MaxLength(11)]
        public string VideoKey
        {
            get;
            set => Set(ref field, value);
        } = "";

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

            _backup = new Episode { Name = Name, VideoKey = VideoKey };
            _inEdit = true;
        }

        public override void CancelEdit()
        {
            if (!_inEdit)
            {
                return;
            }

            Name = _backup!.Name;
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
    }
}