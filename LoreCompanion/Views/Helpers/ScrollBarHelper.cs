using System.Windows;
using MahApps.Metro.IconPacks;

namespace LoreCompanion.Views.Helpers
{
    public static class ScrollBarHelper
    {
        public static readonly DependencyProperty IconKindProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(IconKindProperty)),
            typeof(PackIconMaterialDesignKind),
            typeof(ScrollBarHelper),
            new PropertyMetadata(default(PackIconMaterialDesignKind)));

        public static void SetIconKind(DependencyObject element, PackIconMaterialDesignKind value)
        {
            element.SetValue(IconKindProperty, value);
        }

        public static PackIconMaterialDesignKind GetIconKind(DependencyObject element)
        {
            return (PackIconMaterialDesignKind)element.GetValue(IconKindProperty);
        }
    }
}