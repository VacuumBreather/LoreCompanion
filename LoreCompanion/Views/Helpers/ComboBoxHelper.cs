using System.Windows;

namespace LoreCompanion.Views.Helpers
{
    public static class ComboBoxHelper
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(HeaderProperty)),
            typeof(string),
            typeof(ComboBoxHelper),
            new PropertyMetadata(default(string)));

        public static void SetHeader(DependencyObject element, string value)
        {
            element.SetValue(HeaderProperty, value);
        }

        public static string GetHeader(DependencyObject element)
        {
            return (string)element.GetValue(HeaderProperty);
        }
    }
}