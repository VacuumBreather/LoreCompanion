using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views.Helpers
{
    public static class ListBoxHelper
    {
        public static readonly DependencyProperty ScrollToSelectedItemProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(ScrollToSelectedItemProperty)),
            typeof(bool),
            typeof(ListBoxHelper),
            new PropertyMetadata(false, OnScrollToSelectedItemChanged));

        public static void SetScrollToSelectedItem(DependencyObject element, bool value)
        {
            element.SetValue(ScrollToSelectedItemProperty, value);
        }

        public static bool GetScrollToSelectedItem(DependencyObject element)
        {
            return (bool)element.GetValue(ScrollToSelectedItemProperty);
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: not null } listBox)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        private static void OnScrollToSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                listBox.SelectionChanged += OnSelectionChanged;
            }
            else
            {
                listBox.SelectionChanged -= OnSelectionChanged;
            }
        }
    }
}