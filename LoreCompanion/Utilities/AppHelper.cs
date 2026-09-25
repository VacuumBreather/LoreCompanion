using System.IO;

namespace LoreCompanion.Utilities
{
    public static class AppHelper
    {
        private const string ApplicationName = "LoreCompanion";
        private const string DatabaseFilename = "lorecompanion.db";

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

        private static readonly string LocalAppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ApplicationName);

        public static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ApplicationName);

        public static readonly string CacheFolder = Path.Combine(LocalAppDataFolder, "Cache");
        public static readonly string WebViewUserDataFolder = Path.Combine(LocalAppDataFolder, "WebView2");

        public static readonly string DatabasePath = Path.Combine(AppDataFolder, DatabaseFilename);

        public static readonly string ConnectionString = $"Data Source={DatabasePath}";

        public static readonly bool IsAdminMode = Environment.GetCommandLineArgs().Contains("--admin");

        public static readonly Version CurrentVersion =
            typeof(AppHelper).Assembly.GetName().Version ?? Version.Parse("0.0.0");

        public static readonly string CurrentVersionString = CurrentVersion.ToString();
    }
}