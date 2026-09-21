using System.Windows;

namespace LoreCompanion.Views.Controls
{
    /// <summary>A separating panel supporting limiting the number of displayed elements.</summary>
    public class NotificationPanel : SeparatingPanel
    {
        /// <summary>Identifies the <see cref="MaxDisplayedElements"/> dependency property.</summary>
        public static readonly DependencyProperty MaxDisplayedElementsProperty = DependencyProperty.Register(
            nameof(MaxDisplayedElements),
            typeof(ushort),
            typeof(NotificationPanel),
            new FrameworkPropertyMetadata(
                ushort.MaxValue,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Gets or sets the maximum number of elements the panel should display.</summary>
        public ushort MaxDisplayedElements
        {
            get => (ushort)GetValue(MaxDisplayedElementsProperty);
            set => SetValue(MaxDisplayedElementsProperty, value);
        }

        /// <inheritdoc/>
        protected override IList<UIElement> GetInternalChildElements()
        {
            var maxDisplayed = Math.Clamp(MaxDisplayedElements, 0, InternalChildren.Count);

            return InternalChildren.Cast<UIElement>().Skip(InternalChildren.Count - maxDisplayed).ToArray();
        }
    }
}