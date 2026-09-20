using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views.Helpers
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty IsClearButtonProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(IsClearButtonProperty)),
            typeof(bool),
            typeof(TextBoxHelper),
            new PropertyMetadata(false, OnIsClearButtonChanged));

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

        private static void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { TemplatedParent: TextBox textBox })
            {
                return;
            }

            textBox.SetCurrentValue(TextBox.TextProperty, "");
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
    }
}