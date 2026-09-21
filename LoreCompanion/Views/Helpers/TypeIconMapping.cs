using MahApps.Metro.IconPacks;

namespace LoreCompanion.Views.Helpers
{
    public class TypeIconMapping
    {
        public Type? Type { get; set; }

        public Enum Icon { get; set; } = PackIconMaterialDesignKind.None;
    }
}