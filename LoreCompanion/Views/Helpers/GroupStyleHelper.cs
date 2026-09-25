using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views.Helpers
{
    public static class GroupStyleHelper
    {
        public static readonly DependencyProperty GroupStyleProperty = DependencyProperty.RegisterAttached(
            nameof(GroupStyle),
            typeof(GroupStyle),
            typeof(GroupStyleHelper),
            new PropertyMetadata(null, OnGroupStyleChanged));

        public static GroupStyle? GetGroupStyle(DependencyObject obj)
        {
            return (GroupStyle?)obj.GetValue(GroupStyleProperty);
        }

        public static void SetGroupStyle(DependencyObject obj, GroupStyle? value)
        {
            obj.SetValue(GroupStyleProperty, value);
        }

        private static void OnGroupStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl && e.NewValue is GroupStyle groupStyle)
            {
                itemsControl.GroupStyle.Clear();
                itemsControl.GroupStyle.Add(groupStyle);
            }
        }
    }
}