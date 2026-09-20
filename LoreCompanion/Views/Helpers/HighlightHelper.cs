using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LoreCompanion.Views.Helpers
{
    public static class HighlightHelper
    {
        public static readonly DependencyProperty HighlightTextProperty = DependencyProperty.RegisterAttached(
            "HighlightText",
            typeof(string),
            typeof(HighlightHelper),
            new PropertyMetadata(string.Empty, OnHighlightChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(HighlightHelper),
            new PropertyMetadata(string.Empty, OnHighlightChanged));

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.RegisterAttached(
            "HighlightBrush",
            typeof(Brush),
            typeof(HighlightHelper),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)))); // Gold highlight

        public static readonly DependencyProperty HighlightForegroundBrushProperty =
            DependencyProperty.RegisterAttached(
                "HighlightForegroundBrush",
                typeof(Brush),
                typeof(HighlightHelper),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 120, 0, 0)))); // Gold highlight

        public static string GetHighlightText(DependencyObject obj)
        {
            return (string)obj.GetValue(HighlightTextProperty);
        }

        public static void SetHighlightText(DependencyObject obj, string value)
        {
            obj.SetValue(HighlightTextProperty, value);
        }

        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        public static Brush GetHighlightBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightBrushProperty);
        }

        public static void SetHighlightBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightBrushProperty, value);
        }

        public static Brush GetHighlightForegroundBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightForegroundBrushProperty);
        }

        public static void SetHighlightForegroundBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightForegroundBrushProperty, value);
        }

        private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock)
            {
                return;
            }

            var text = GetText(textBlock);
            var query = GetHighlightText(textBlock);
            var highlightBrush = GetHighlightBrush(textBlock);
            var highlightForegroundBrush = GetHighlightForegroundBrush(textBlock);

            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (string.IsNullOrEmpty(query))
            {
                textBlock.Inlines.Add(new Run(text));

                return;
            }

            var currentIndex = 0;

            while (currentIndex < text.Length)
            {
                var matchIndex = text.IndexOf(query, currentIndex, StringComparison.OrdinalIgnoreCase);

                if (matchIndex < 0)
                {
                    // Add remaining unmatched text
                    textBlock.Inlines.Add(new Run(text.Substring(currentIndex)));

                    break;
                }

                // Add unhighlighted chunk before the match
                if (matchIndex > currentIndex)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(currentIndex, matchIndex - currentIndex)));
                }

                // Add highlighted chunk
                var matchText = text.Substring(matchIndex, query.Length);

                var matchRun = new Run(matchText)
                {
                    Background = highlightBrush, Foreground = highlightForegroundBrush,
                };

                textBlock.Inlines.Add(matchRun);

                currentIndex = matchIndex + query.Length;
            }
        }
    }
}