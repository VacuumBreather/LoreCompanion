using System.Windows.Markup;

namespace LoreCompanion.Views.MarkupExtensions
{
    public class LetterSpacingExtension(string str) : MarkupExtension
    {
        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return string.Join("\u200A", str.ToCharArray());
        }
    }
}