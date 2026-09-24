using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views.Helpers
{
    public static class ComboBoxHelper
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(HeaderProperty)),
            typeof(string),
            typeof(ComboBoxHelper),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ClearButtonVisibilityProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(ClearButtonVisibilityProperty)),
            typeof(Visibility),
            typeof(ComboBoxHelper),
            new PropertyMetadata(Visibility.Collapsed));

        public static void SetHeader(DependencyObject element, string value)
        {
            element.SetValue(HeaderProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static string GetHeader(DependencyObject element)
        {
            return (string)element.GetValue(HeaderProperty);
        }

        public static void SetClearButtonVisibility(DependencyObject element, Visibility value)
        {
            element.SetValue(ClearButtonVisibilityProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static Visibility GetClearButtonVisibility(DependencyObject element)
        {
            return (Visibility)element.GetValue(ClearButtonVisibilityProperty);
        }
    }
}