using System.Windows.Markup;
using LoreCompanion.Models;

namespace LoreCompanion.Views.MarkupExtensions
{
    public class ItemTypesExtension : MarkupExtension
    {
        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues<ItemType>();
        }
    }
}