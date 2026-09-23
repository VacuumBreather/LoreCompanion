using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LoreCompanion.Extensions;
using Microsoft.Xaml.Behaviors;

namespace LoreCompanion.Views.Behaviors
{
    public class TimeTextBoxBehavior : Behavior<TextBox>
    {
        private const string DefaultTime = "00:00:00";

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(TimeSpan),
            typeof(TimeTextBoxBehavior),
            new FrameworkPropertyMetadata(
                TimeSpan.Zero,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

        private bool _isUpdating;

        public TimeSpan Value
        {
            get => (TimeSpan)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static TimeSpan MinTimeSpan => TimeSpan.Zero;

        private static TimeSpan MaxTimeSpan { get; } = new(99, 59, 59);

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is null)
            {
                return;
            }

            AssociatedObject.MaxLength = 8;
            UpdateTextFromValue(Value);

            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.LostFocus += OnLostFocus;
            DataObject.AddPastingHandler(AssociatedObject, OnPaste);
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is not null)
            {
                AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
                AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
                AssociatedObject.SelectionChanged -= OnSelectionChanged;
                AssociatedObject.LostFocus -= OnLostFocus;
                DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
            }

            base.OnDetaching();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (!TryParseTime(AssociatedObject.Text, out var _))
            {
                UpdateTextFromValue(Value);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Space:
                    e.Handled = true;

                    return;

                case Key.Back:
                    e.Handled = true;
                    HandleBackspace();

                    return;

                case Key.Delete:
                    e.Handled = true;
                    HandleDelete();

                    return;

                case Key.Left when Keyboard.Modifiers == ModifierKeys.None:
                    e.Handled = true;
                    HandleLeftArrow();

                    return;

                case Key.Right when Keyboard.Modifiers == ModifierKeys.None:
                    e.Handled = true;
                    HandleRightArrow();

                    return;
            }
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject is null || _isUpdating)
            {
                return;
            }

            if ((AssociatedObject.SelectionLength != 0) || AssociatedObject.SelectionStart is not (2 or 5))
            {
                return;
            }

            _isUpdating = true;

            try
            {
                AssociatedObject.SelectionStart += 1;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrEmpty(e.Text))
            {
                return;
            }

            ApplyInputText(e.Text);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
            e.Handled = true;

            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                return;
            }

            if (e.DataObject.GetData(DataFormats.Text) is not string text || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            ApplyInputText(text.Trim());
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeTextBoxBehavior behavior)
            {
                behavior.UpdateTextFromValue(e.NewValue is TimeSpan value ? value : null);
            }
        }

        private static string FormatTimeSpan(TimeSpan time)
        {
            time = time.Clamp(MinTimeSpan, MaxTimeSpan);

            var totalHours = (int)time.TotalHours;
            var minutes = time.Minutes;
            var seconds = time.Seconds;

            return $"{totalHours:D2}:{minutes:D2}:{seconds:D2}";
        }

        private static string EnsureValidFormat(string? text)
        {
            if (string.IsNullOrWhiteSpace(text) || (text.Length != 8) || (text[2] != ':') || (text[5] != ':'))
            {
                return DefaultTime;
            }

            return text;
        }

        private static bool TryParseTime(string text, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(text) || (text.Length != 8) || (text[2] != ':') || (text[5] != ':'))
            {
                return false;
            }

            if (!int.TryParse(text.AsSpan(0, 2), out var hours) ||
                !int.TryParse(text.AsSpan(3, 2), out var minutes) ||
                !int.TryParse(text.AsSpan(6, 2), out var seconds))
            {
                return false;
            }

            if (hours is < 0 or > 99 || minutes is < 0 or > 59 || seconds is < 0 or > 59)
            {
                return false;
            }

            timeSpan = new TimeSpan(hours, minutes, seconds);

            return true;
        }

        private void HandleLeftArrow()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.SelectionLength > 0)
            {
                var start = AssociatedObject.SelectionStart;

                if (start is 2 or 5)
                {
                    start--;
                }

                AssociatedObject.SelectionStart = Math.Max(start, 0);
                AssociatedObject.SelectionLength = 0;

                return;
            }

            var caret = AssociatedObject.SelectionStart;

            var nextCaret = caret switch
            {
                <= 0 => 0,
                1 => 0,
                2 => 1,
                3 => 1,
                4 => 3,
                5 => 4,
                6 => 4,
                7 => 6,
                var _ => 7,
            };

            AssociatedObject.SelectionStart = nextCaret;
        }

        private void HandleRightArrow()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.SelectionLength > 0)
            {
                var end = AssociatedObject.SelectionStart + AssociatedObject.SelectionLength;

                if (end is 2 or 5)
                {
                    end++;
                }

                AssociatedObject.SelectionStart = Math.Min(end, 8);
                AssociatedObject.SelectionLength = 0;

                return;
            }

            var caret = AssociatedObject.SelectionStart;

            var nextCaret = caret switch
            {
                0 => 1,
                1 => 3,
                2 => 3,
                3 => 4,
                4 => 6,
                5 => 6,
                6 => 7,
                7 => 8,
                var _ => 8,
            };

            AssociatedObject.SelectionStart = nextCaret;
        }

        private void HandleBackspace()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            var currentText = EnsureValidFormat(AssociatedObject.Text);
            var chars = currentText.ToCharArray();
            var selStart = AssociatedObject.SelectionStart;
            var selLength = AssociatedObject.SelectionLength;

            if (selLength > 0)
            {
                var selEnd = Math.Min(selStart + selLength, 8);

                for (var i = selStart; i < selEnd; i++)
                {
                    if (i is not (2 or 5))
                    {
                        chars[i] = '0';
                    }
                }

                SetTextAndCaret(chars, selStart);
                SyncValueProperty();

                return;
            }

            if (selStart == 0)
            {
                return;
            }

            var target = selStart - 1;

            if (target is 2 or 5)
            {
                target--;
            }

            if (target < 0)
            {
                return;
            }

            chars[target] = '0';
            SetTextAndCaret(chars, target);
            SyncValueProperty();
        }

        private void HandleDelete()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            var currentText = EnsureValidFormat(AssociatedObject.Text);
            var chars = currentText.ToCharArray();
            var selStart = AssociatedObject.SelectionStart;
            var selLength = AssociatedObject.SelectionLength;

            if (selLength > 0)
            {
                var selEnd = Math.Min(selStart + selLength, 8);

                for (var i = selStart; i < selEnd; i++)
                {
                    if (i is not (2 or 5))
                    {
                        chars[i] = '0';
                    }
                }

                SetTextAndCaret(chars, selStart);
                SyncValueProperty();

                return;
            }

            var target = selStart;

            if (target is 2 or 5)
            {
                target++;
            }

            if (target >= 8)
            {
                return;
            }

            chars[target] = '0';
            var nextCaret = target + 1;

            if (nextCaret is 2 or 5)
            {
                nextCaret++;
            }

            SetTextAndCaret(chars, Math.Min(nextCaret, 8));
            SyncValueProperty();
        }

        private void ApplyInputText(string input)
        {
            if (AssociatedObject is null || string.IsNullOrEmpty(input))
            {
                return;
            }

            // If input contains colons (e.g., pasted "99:99:99" or "9:9:9"), parse segmented parts with clamping
            if (input.Contains(':'))
            {
                ApplySegmentedTime(input);

                return;
            }

            // Sequential slot typing / partial paste
            var currentText = EnsureValidFormat(AssociatedObject.Text);
            var chars = currentText.ToCharArray();
            var selStart = AssociatedObject.SelectionStart;
            var selLength = AssociatedObject.SelectionLength;

            if (selLength > 0)
            {
                var selEnd = Math.Min(selStart + selLength, 8);

                for (var i = selStart; i < selEnd; i++)
                {
                    if (i is not (2 or 5))
                    {
                        chars[i] = '0';
                    }
                }
            }

            var caret = selStart;

            if (caret is 2 or 5)
            {
                caret++;
            }

            foreach (var c in input.TakeWhile(_ => caret < 8))
            {
                if (c == ':')
                {
                    switch (caret)
                    {
                        case 2 or 5:
                            caret++;

                            break;

                        case 0:
                            caret = 3;

                            break;

                        case 1:
                            chars[1] = chars[0];
                            chars[0] = '0';
                            caret = 3;

                            break;

                        case 3:
                            caret = 6;

                            break;

                        case 4:
                            chars[4] = chars[3];
                            chars[3] = '0';
                            caret = 6;

                            break;
                    }

                    continue;
                }

                if (!char.IsDigit(c))
                {
                    continue;
                }

                if (caret is 2 or 5)
                {
                    caret++;
                }

                if (caret >= 8)
                {
                    break;
                }

                if (caret is 3 or 6 && (c > '5'))
                {
                    chars[caret] = '0';
                    caret++;
                }

                chars[caret] = c;
                caret++;
            }

            if (caret is 2 or 5)
            {
                caret++;
            }

            SetTextAndCaret(chars, Math.Min(caret, 8));
            SyncValueProperty();
        }

        private void ApplySegmentedTime(string input)
        {
            var parts = input.Split(':');
            var h = 0;
            var m = 0;
            var s = 0;

            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out var parsedM))
                {
                    m = Math.Clamp(parsedM, 0, 59);
                }

                if (int.TryParse(parts[1], out var parsedS))
                {
                    s = Math.Clamp(parsedS, 0, 59);
                }
            }
            else
            {
                if ((parts.Length > 0) && int.TryParse(parts[0], out var parsedH))
                {
                    h = Math.Clamp(parsedH, 0, 99);
                }

                if ((parts.Length > 1) && int.TryParse(parts[1], out var parsedM))
                {
                    m = Math.Clamp(parsedM, 0, 59);
                }

                if ((parts.Length > 2) && int.TryParse(parts[2], out var parsedS))
                {
                    s = Math.Clamp(parsedS, 0, 59);
                }
            }

            var formatted = $"{h:D2}:{m:D2}:{s:D2}";
            SetTextAndCaret(formatted.ToCharArray(), 8);
            SyncValueProperty();
        }

        private void SetTextAndCaret(char[] chars, int caret)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (caret is 2 or 5)
            {
                caret++;
            }

            _isUpdating = true;

            try
            {
                AssociatedObject.Text = new string(chars);
                AssociatedObject.SelectionStart = Math.Min(caret, 8);
                AssociatedObject.SelectionLength = 0;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateTextFromValue(TimeSpan? time)
        {
            if (AssociatedObject is null || _isUpdating)
            {
                return;
            }

            var formatted = time.HasValue ? FormatTimeSpan(time.Value) : DefaultTime;

            if (AssociatedObject.Text == formatted)
            {
                return;
            }

            _isUpdating = true;

            try
            {
                AssociatedObject.Text = formatted;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void SyncValueProperty()
        {
            if (AssociatedObject is null || _isUpdating)
            {
                return;
            }

            if (!TryParseTime(AssociatedObject.Text, out var parsed))
            {
                return;
            }

            _isUpdating = true;

            try
            {
                SetCurrentValue(ValueProperty, parsed);
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}