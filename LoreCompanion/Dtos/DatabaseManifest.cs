using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace LoreCompanion.Dtos
{
    [PublicAPI]
    public record DatabaseManifest
    {
        [JsonPropertyName("version")]
        public Version? Version { get; set; }

        [JsonPropertyName("sha256")]
        public string? Sha256 { get; set; }

        [JsonPropertyName("required_app_version")]
        public Version? RequiredAppVersion { get; set; }

        [JsonPropertyName("downloadUrl")]
        public Uri? DownloadUrl { get; set; }
    }
}