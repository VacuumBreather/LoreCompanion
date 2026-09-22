using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using LoreCompanion.ViewModels.Dialogs;

namespace LoreCompanion.Views.Controls
{
    /// <summary>Contains attached dependency properties used when closing a dialog.</summary>
    public static class CloseDialog
    {
        private const string CloseDialogAsync = nameof(DialogScreen.CloseDialogAsync);

        private static readonly string Click = nameof(ButtonBase.ClickEvent).Replace("Event", string.Empty);

        /// <summary>
        ///     Result property. This is an attached property. CloseDialog defines the Result, so that it can be set on any
        ///     <see cref="Button"/> that is used to close a dialog with that result.
        /// </summary>
        public static readonly DependencyProperty ResultProperty = DependencyProperty.RegisterAttached(
            "Result",
            typeof(DialogResult),
            typeof(CloseDialog),
            new PropertyMetadata(default(DialogResult), OnResultChanged));

        /// <summary>Gets the result to close a dialog with.</summary>
        /// <param name="button">The button which sets the dialog result.</param>
        /// <returns>The dialog result the dialog will be closed with.</returns>
        public static DialogResult GetResult(Button button)
        {
            return (DialogResult)button.GetValue(ResultProperty);
        }

        /// <summary>Sets the result to close a dialog with.</summary>
        /// <param name="button">The button which sets the dialog result.</param>
        /// <param name="result">The dialog result to close the dialog with.</param>
        public static void SetResult(Button button, DialogResult result)
        {
            button.SetValue(ResultProperty, result);
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Button button)
            {
                Attach(button);
            }
        }

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button button)
            {
                return;
            }

            button.DataContextChanged -= OnDataContextChanged;
            button.DataContextChanged += OnDataContextChanged;

            Attach(button);
        }

        private static void Attach(Button button)
        {
            Message.SetAttach(button, string.Empty);

            var result = GetResult(button).ToString().ToUpperInvariant();

            Message.SetAttach(button, $"[Event {Click}] = [Action {CloseDialogAsync}('{result}')]");
        }
    }
}