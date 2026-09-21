using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoreCompanion.Views.Controls
{
    /// <summary>A <see cref="Decorator"/> which allows to overlay dialogs and notifications.</summary>
    public class DialogsNotificationsDecorator : Decorator
    {
        private readonly DialogHost _dialogHost = new();
        private readonly NotificationHost _notificationHost = new();

        public DialogsNotificationsDecorator()
        {
            AddLogicalChild(_notificationHost);
            AddLogicalChild(_dialogHost);

            AddVisualChild(_notificationHost);
            AddVisualChild(_dialogHost);
        }

        /// <summary>Gets the <see cref="Visual"/> children count.</summary>
        protected override int VisualChildrenCount => (Child is null ? 0 : 1) + 2;

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (Child is not null)
                {
                    yield return Child;
                }

                yield return _notificationHost;
                yield return _dialogHost;
            }
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            var finalSizeRect = new Rect(finalSize);

            _notificationHost.Arrange(finalSizeRect);
            _dialogHost.Arrange(finalSizeRect);

            return finalSize;
        }

        /// <inheritdoc/>
        protected override Visual GetVisualChild(int index)
        {
            if (Child is not null)
            {
                return index switch
                {
                    0 => Child,
                    1 => _notificationHost,
                    2 => _dialogHost,
                    var _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }

            return index switch
            {
                0 => _notificationHost,
                1 => _dialogHost,
                var _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = base.MeasureOverride(constraint);
            _notificationHost.Measure(constraint);
            _dialogHost.Measure(constraint);

            desiredSize = new Size(
                Math.Max(
                    Math.Max(desiredSize.Width, _notificationHost.DesiredSize.Width),
                    _dialogHost.DesiredSize.Width),
                Math.Max(
                    Math.Max(desiredSize.Height, _notificationHost.DesiredSize.Height),
                    _dialogHost.DesiredSize.Height));

            return desiredSize;
        }
    }
}