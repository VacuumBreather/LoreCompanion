using System.Text.Json.Serialization;

namespace LoreCompanion.ViewModels
{
    public class DatabaseManifest
    {
        [JsonPropertyName("version")]
        public Version Version { get; set; }

        [JsonPropertyName("downloadUrl")]
        public Uri DownloadUrl  { get; set; }
    }
}