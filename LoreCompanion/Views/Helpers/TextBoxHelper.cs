using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views.Helpers
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(HeaderProperty)),
            typeof(string),
            typeof(TextBoxHelper),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IsClearButtonProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(IsClearButtonProperty)),
            typeof(bool),
            typeof(TextBoxHelper),
            new PropertyMetadata(false, OnIsClearButtonChanged));

        public static void SetHeader(DependencyObject element, string value)
        {
            element.SetValue(HeaderProperty, value);
        }

        public static string GetHeader(DependencyObject element)
        {
            return (string)element.GetValue(HeaderProperty);
        }

        public static void SetIsClearButton(Button button, bool value)
        {
            button.SetValue(IsClearButtonProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static bool GetIsClearButton(Button button)
        {
            return (bool)button.GetValue(IsClearButtonProperty);
        }

        private static void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { TemplatedParent: TextBox textBox })
            {
                return;
            }

            textBox.SetCurrentValue(TextBox.TextProperty, "");
        }

        private static void OnIsClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button button)
            {
                return;
            }

            button.Click -= OnClearButtonClick;

            if (e is { NewValue: true })
            {
                button.Click += OnClearButtonClick;
            }
        }
    }
}