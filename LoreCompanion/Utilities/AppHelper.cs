using System.IO;

namespace LoreCompanion.Utilities
{
    public static class AppHelper
    {
        public static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LoreCompanion");

        public static readonly string DatabasePath = Path.Combine(AppDataFolder, "lorecompanion.db");

        public static readonly string ConnectionString = $"Data Source={DatabasePath}";

        public static readonly bool IsAdminMode = Environment.GetCommandLineArgs().Contains("--admin");

        public static readonly string DatabaseManifestUrl =
            "https://raw.githubusercontent.com/VacuumBreather/LoreCompanion/refs/heads/database/database_manifest.json";

        public static readonly Version CurrentVersion =
            typeof(AppHelper).Assembly.GetName().Version ?? Version.Parse("0.0.0");
    }
}