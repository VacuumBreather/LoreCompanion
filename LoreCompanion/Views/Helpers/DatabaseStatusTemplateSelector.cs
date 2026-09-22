using System.Windows;
using System.Windows.Controls;
using LoreCompanion.ViewModels;

namespace LoreCompanion.Views.Helpers
{
    public class DatabaseStatusTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UnknownTemplate { get; set; }

        public DataTemplate? UpToDateTemplate { get; set; }

        public DataTemplate? OutOfDateTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is not DatabaseStatus status)
            {
                return null;
            }

            return status switch
            {
                DatabaseStatus.Unknown => UnknownTemplate,
                DatabaseStatus.UpToDate => UpToDateTemplate,
                DatabaseStatus.OutOfDate => OutOfDateTemplate,
                var _ => null,
            };
        }
    }
}