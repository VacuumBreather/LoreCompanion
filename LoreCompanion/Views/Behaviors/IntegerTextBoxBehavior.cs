using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace LoreCompanion.Views.Behaviors
{
    public class IntegerTextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(IntegerTextBoxBehavior),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(int),
            typeof(IntegerTextBoxBehavior),
            new PropertyMetadata(0, OnBoundsChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(int),
            typeof(IntegerTextBoxBehavior),
            new PropertyMetadata(int.MaxValue, OnBoundsChanged));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(int),
            typeof(IntegerTextBoxBehavior),
            new PropertyMetadata(1));

        public static readonly DependencyProperty EnableMouseWheelProperty = DependencyProperty.Register(
            nameof(EnableMouseWheel),
            typeof(bool),
            typeof(IntegerTextBoxBehavior),
            new PropertyMetadata(true));

        private bool _isUpdating;

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public bool EnableMouseWheel
        {
            get => (bool)GetValue(EnableMouseWheelProperty);
            set => SetValue(EnableMouseWheelProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is null)
            {
                return;
            }

            UpdateTextFromValue(Value);

            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.MouseWheel += OnMouseWheel;
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.LostFocus += OnLostFocus;
            DataObject.AddPastingHandler(AssociatedObject, OnPaste);
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is not null)
            {
                AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
                AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
                AssociatedObject.MouseWheel -= OnMouseWheel;
                AssociatedObject.TextChanged -= OnTextChanged;
                AssociatedObject.LostFocus -= OnLostFocus;
                DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
            }

            base.OnDetaching();
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (AssociatedObject is null || AssociatedObject.IsReadOnly)
            {
                return;
            }

            var fullText = GetProjectedText(e.Text);
            e.Handled = !IsValidIntegerText(fullText);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AssociatedObject is null || AssociatedObject.IsReadOnly)
            {
                return;
            }

            // Reject spaces
            if (e.Key == Key.Space)
            {
                e.Handled = true;

                return;
            }

            // Up / Down arrow step increment/decrement
            if (e.Key == Key.Up)
            {
                StepValue(Step);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                StepValue(-Step);
                e.Handled = true;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (AssociatedObject is null ||
                AssociatedObject.IsReadOnly ||
                !EnableMouseWheel ||
                !AssociatedObject.IsKeyboardFocusWithin)
            {
                return;
            }

            StepValue(e.Delta > 0 ? Step : -Step);
            e.Handled = true;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (AssociatedObject is null || AssociatedObject.IsReadOnly)
            {
                return;
            }

            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();

                return;
            }

            var pasteText = (string)(e.DataObject.GetData(DataFormats.Text) ?? "");
            var projected = GetProjectedText(pasteText);

            if (!IsValidIntegerText(projected))
            {
                e.CancelCommand();
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (AssociatedObject is null || _isUpdating)
            {
                return;
            }

            var text = AssociatedObject.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                _isUpdating = true;
                Value = Math.Max(0, Minimum);
                _isUpdating = false;

                return;
            }

            if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            {
                _isUpdating = true;
                Value = Clamp(parsed);
                _isUpdating = false;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            var text = AssociatedObject.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                UpdateTextFromValue(Value);

                return;
            }

            if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            {
                var clamped = Clamp(parsed);

                if (clamped != Value)
                {
                    Value = clamped;
                }

                UpdateTextFromValue(clamped);
            }
            else
            {
                UpdateTextFromValue(Value);
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IntegerTextBoxBehavior behavior)
            {
                behavior.UpdateTextFromValue((int)e.NewValue);
            }
        }

        private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IntegerTextBoxBehavior behavior)
            {
                var clamped = behavior.Clamp(behavior.Value);

                if (clamped != behavior.Value)
                {
                    behavior.Value = clamped;
                }
            }
        }

        private void StepValue(int delta)
        {
            var next = Clamp(Value + delta);

            Value = next;
            UpdateTextFromValue(next);

            if (AssociatedObject is not null)
            {
                AssociatedObject.SelectAll();
            }
        }

        private int Clamp(int val)
        {
            var effectiveMin = Math.Max(0, Minimum);

            if (val < effectiveMin)
            {
                return effectiveMin;
            }

            if (val > Maximum)
            {
                return Maximum;
            }

            return val;
        }

        private bool IsValidIntegerText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && (parsed >= 0);
        }

        private string GetProjectedText(string input)
        {
            if (AssociatedObject is null)
            {
                return input;
            }

            var current = AssociatedObject.Text;
            var start = AssociatedObject.SelectionStart;
            var length = AssociatedObject.SelectionLength;

            return current.Remove(start, length).Insert(start, input);
        }

        private void UpdateTextFromValue(int val)
        {
            if (AssociatedObject is null || _isUpdating)
            {
                return;
            }

            _isUpdating = true;
            AssociatedObject.Text = val.ToString(CultureInfo.InvariantCulture);
            _isUpdating = false;
        }
    }
}