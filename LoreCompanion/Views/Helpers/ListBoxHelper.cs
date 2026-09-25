using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LoreCompanion.Views.Helpers
{
    public static class ListBoxHelper
    {
        public static readonly DependencyProperty ScrollToSelectedItemProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(ScrollToSelectedItemProperty)),
            typeof(bool),
            typeof(ListBoxHelper),
            new PropertyMetadata(false, OnScrollToSelectedItemChanged));

        private static readonly DependencyProperty ItemsCollectionChangedHandlerProperty =
            DependencyProperty.RegisterAttached(
                DependencyPropertyNameHelper.GetName(nameof(ItemsCollectionChangedHandlerProperty)),
                typeof(NotifyCollectionChangedEventHandler),
                typeof(ListBoxHelper),
                new PropertyMetadata(null));

        public static void SetScrollToSelectedItem(DependencyObject element, bool value)
        {
            element.SetValue(ScrollToSelectedItemProperty, value);
        }

        public static bool GetScrollToSelectedItem(DependencyObject element)
        {
            return (bool)element.GetValue(ScrollToSelectedItemProperty);
        }

        private static void OnListBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                SubscribeToItemsCollectionChanged(listBox);
                ScrollSelectedItemIntoView(listBox);
            }
        }

        private static void OnListBoxUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                UnsubscribeFromItemsCollectionChanged(listBox);
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                ScrollSelectedItemIntoView(listBox);
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
                listBox.Loaded += OnListBoxLoaded;
                listBox.Unloaded += OnListBoxUnloaded;

                if (listBox.IsLoaded)
                {
                    SubscribeToItemsCollectionChanged(listBox);
                }
            }
            else
            {
                listBox.SelectionChanged -= OnSelectionChanged;
                listBox.Loaded -= OnListBoxLoaded;
                listBox.Unloaded -= OnListBoxUnloaded;
                UnsubscribeFromItemsCollectionChanged(listBox);
            }
        }

        private static void SubscribeToItemsCollectionChanged(ListBox listBox)
        {
            UnsubscribeFromItemsCollectionChanged(listBox);

            if (listBox.Items is not INotifyCollectionChanged notifyCollection)
            {
                return;
            }

            NotifyCollectionChangedEventHandler handler = (_, _) => ScrollSelectedItemIntoView(listBox);
            listBox.SetValue(ItemsCollectionChangedHandlerProperty, handler);
            notifyCollection.CollectionChanged += handler;
        }

        private static void UnsubscribeFromItemsCollectionChanged(ListBox listBox)
        {
            if (listBox.GetValue(ItemsCollectionChangedHandlerProperty) is not NotifyCollectionChangedEventHandler
                handler)
            {
                return;
            }

            if (listBox.Items is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged -= handler;
            }

            listBox.ClearValue(ItemsCollectionChangedHandlerProperty);
        }

        private static void ScrollSelectedItemIntoView(ListBox listBox)
        {
            if (listBox.SelectedItem is null)
            {
                return;
            }

            // Defer execution until after the layout pass / container generation is complete
            listBox.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                () =>
                {
                    if (listBox.SelectedItem is not null)
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }
                });
        }
    }
}