using System.IO;

namespace LoreCompanion.Models
{
    public static class AppHelper
    {
        public static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LoreCompanion");

        public static readonly string DatabasePath = Path.Combine(AppDataFolder, "lorecompanion.db");

        public static readonly string ConnectionString = $"Data Source={DatabasePath}";
    }
}