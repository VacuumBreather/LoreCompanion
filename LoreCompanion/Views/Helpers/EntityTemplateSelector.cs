using System.Windows;
using System.Windows.Controls;
using LoreCompanion.Models;

namespace LoreCompanion.Views.Helpers
{
    public class EntityTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? EpisodeTemplate { get; set; }

        public DataTemplate? LocationTemplate { get; set; }

        public DataTemplate? CharacterTemplate { get; set; }

        public DataTemplate? ItemTemplate { get; set; }

        public DataTemplate? DialogTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is not EntityBase entity)
            {
                return null;
            }

            return entity switch
            {
                Episode _ => EpisodeTemplate,
                Location _ => LocationTemplate,
                Character _ => CharacterTemplate,
                Item _ => ItemTemplate,
                Dialog _ => DialogTemplate,
                var _ => null,
            };
        }
    }
}