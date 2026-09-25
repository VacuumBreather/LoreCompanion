using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views.Controls
{
    /// <summary>Represents a selectable dialog item inside a <see cref="DialogHost"/>.</summary>
    /// <seealso cref="ContentControl"/>
    public class DialogItem : ContentControl
    {
        /// <summary>Initializes static members of the <see cref="DialogItem"/> class.</summary>
        static DialogItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DialogItem),
                new FrameworkPropertyMetadata(typeof(DialogItem)));
        }
    }
}