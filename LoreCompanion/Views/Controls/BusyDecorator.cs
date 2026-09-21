using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoreCompanion.Views.Controls
{
    /// <summary>
    /// Overlays wrapped content with a semi-transparent background and customizable busy indicator.
    /// </summary>
    [TemplatePart(Name = OverlayGridPartName, Type = typeof(Grid))]
    public class BusyDecorator : ContentControl
    {
        public const string OverlayGridPartName = "PART_OverlayGrid";

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(BusyDecorator),
            new PropertyMetadata(false));

        public static readonly DependencyProperty BusyContentProperty = DependencyProperty.Register(
            nameof(BusyContent),
            typeof(object),
            typeof(BusyDecorator),
            new PropertyMetadata(null));

        public static readonly DependencyProperty BusyContentTemplateProperty = DependencyProperty.Register(
            nameof(BusyContentTemplate),
            typeof(DataTemplate),
            typeof(BusyDecorator),
            new PropertyMetadata(null));

        public static readonly DependencyProperty OverlayBackgroundBrushProperty = DependencyProperty.Register(
            nameof(OverlayBackgroundBrush),
            typeof(Brush),
            typeof(BusyDecorator),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(160, 20, 20, 20))));

        static BusyDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BusyDecorator),
                new FrameworkPropertyMetadata(typeof(BusyDecorator)));
        }

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public object? BusyContent
        {
            get => GetValue(BusyContentProperty);
            set => SetValue(BusyContentProperty, value);
        }

        public DataTemplate? BusyContentTemplate
        {
            get => (DataTemplate?)GetValue(BusyContentTemplateProperty);
            set => SetValue(BusyContentTemplateProperty, value);
        }

        public Brush OverlayBackgroundBrush
        {
            get => (Brush)GetValue(OverlayBackgroundBrushProperty);
            set => SetValue(OverlayBackgroundBrushProperty, value);
        }
    }
}