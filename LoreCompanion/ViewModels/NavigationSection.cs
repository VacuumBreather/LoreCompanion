namespace LoreCompanion.ViewModels
{
    public static class NavigationSection
    {
        public static readonly string Overview = "Overview";
        public static readonly string Lore = "Lore";
        public static readonly string Progress = "Progress";

        public static readonly IReadOnlyList<string> Order =
        [
            Overview,
            Lore,
            Progress,
        ];
    }
}