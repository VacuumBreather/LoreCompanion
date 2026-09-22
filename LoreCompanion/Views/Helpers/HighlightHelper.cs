using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LoreCompanion.Views.Helpers
{
    public static class HighlightHelper
    {
        public static readonly DependencyProperty HighlightTextProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(HighlightTextProperty)),
            typeof(string),
            typeof(HighlightHelper),
            new PropertyMetadata(string.Empty, OnHighlightChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(TextProperty)),
            typeof(string),
            typeof(HighlightHelper),
            new PropertyMetadata(string.Empty, OnHighlightChanged));

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(HighlightBrushProperty)),
            typeof(Brush),
            typeof(HighlightHelper),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)))); // Gold highlight

        public static readonly DependencyProperty HighlightForegroundBrushProperty =
            DependencyProperty.RegisterAttached(
                DependencyPropertyNameHelper.GetName(nameof(HighlightForegroundBrushProperty)),
                typeof(Brush),
                typeof(HighlightHelper),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 120, 0, 0)))); // Dark red highlight

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

        public static void UpdateHighlight(TextBlock textBlock)
        {
            var text = GetText(textBlock);
            var query = GetHighlightText(textBlock);
            var highlightBrush = GetHighlightBrush(textBlock);
            var highlightForegroundBrush = GetHighlightForegroundBrush(textBlock);

            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var allCaps = TypographyHelper.GetAllCaps(textBlock);
            var letterSpacing = TypographyHelper.GetIncreaseLetterSpacing(textBlock);

            if (string.IsNullOrEmpty(query))
            {
                var formattedText = TypographyHelper.TransformText(text, allCaps, letterSpacing);
                textBlock.Inlines.Add(new Run(formattedText));

                return;
            }

            var currentIndex = 0;

            while (currentIndex < text.Length)
            {
                var matchIndex = text.IndexOf(query, currentIndex, StringComparison.OrdinalIgnoreCase);

                if (matchIndex < 0)
                {
                    // Add remaining unmatched text
                    var remaining = text.Substring(currentIndex);
                    var transformed = TransformChunk(remaining, allCaps, letterSpacing, true);
                    textBlock.Inlines.Add(new Run(transformed));

                    break;
                }

                // Add unhighlighted chunk before the match
                if (matchIndex > currentIndex)
                {
                    var chunk = text.Substring(currentIndex, matchIndex - currentIndex);
                    var transformed = TransformChunk(chunk, allCaps, letterSpacing, false);
                    textBlock.Inlines.Add(new Run(transformed));
                }

                // Add highlighted chunk
                var matchEnd = matchIndex + query.Length;
                var matchText = text.Substring(matchIndex, query.Length);
                var transformedMatch = TransformChunk(matchText, allCaps, letterSpacing, matchEnd >= text.Length);

                var matchRun = new Run(transformedMatch)
                {
                    Background = highlightBrush, Foreground = highlightForegroundBrush,
                };

                textBlock.Inlines.Add(matchRun);

                currentIndex = matchEnd;
            }
        }

        private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                UpdateHighlight(textBlock);
            }
        }

        private static string TransformChunk(string chunk, bool allCaps, bool letterSpacing, bool isEndOfString)
        {
            var transformed = TypographyHelper.TransformText(chunk, allCaps, letterSpacing);

            // Append hair space if more characters follow in subsequent runs
            if (letterSpacing && !isEndOfString && !string.IsNullOrEmpty(transformed))
            {
                transformed += "\u200A";
            }

            return transformed;
        }
    }
}