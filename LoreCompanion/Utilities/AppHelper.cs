using System.IO;

namespace LoreCompanion.Utilities
{
    public static class AppHelper
    {
#if DEBUG
        public const string ApplicationManifestUrl =
            "https://raw.githubusercontent.com/VacuumBreather/LoreCompanion/refs/heads/development/application_manifest.json";

        public const string DatabaseManifestUrl =
            "https://raw.githubusercontent.com/VacuumBreather/LoreCompanion/refs/heads/database_development/database_manifest.json";
#else
        public const string ApplicationManifestUrl =
            "https://raw.githubusercontent.com/VacuumBreather/LoreCompanion/refs/heads/master/application_manifest.json";

        public const string DatabaseManifestUrl =
            "https://raw.githubusercontent.com/VacuumBreather/LoreCompanion/refs/heads/database/database_manifest.json";
#endif

        public static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LoreCompanion");

        public static readonly string DatabasePath = Path.Combine(AppDataFolder, "lorecompanion.db");

        public static readonly string ConnectionString = $"Data Source={DatabasePath}";

        public static readonly bool IsAdminMode = Environment.GetCommandLineArgs().Contains("--admin");

        public static readonly Version CurrentVersion =
            typeof(AppHelper).Assembly.GetName().Version ?? Version.Parse("0.0.0");

        public static readonly string CurrentVersionString = CurrentVersion.ToString();
    }
}