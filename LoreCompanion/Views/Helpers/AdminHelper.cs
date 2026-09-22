using System.Windows;

namespace LoreCompanion.Views.Helpers
{
    public static class AdminHelper
    {
        public static readonly DependencyProperty IsAdminModeProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(IsAdminModeProperty)),
            typeof(bool),
            typeof(AdminHelper),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetIsAdminMode(DependencyObject element, bool value)
        {
            element.SetValue(IsAdminModeProperty, value);
        }

        public static bool GetIsAdminMode(DependencyObject element)
        {
            return (bool)element.GetValue(IsAdminModeProperty);
        }
    }
}