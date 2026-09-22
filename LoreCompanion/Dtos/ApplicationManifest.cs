using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace LoreCompanion.Dtos
{
    [PublicAPI]
    public record ApplicationManifest
    {
        [JsonPropertyName("version")]
        public Version? Version { get; set; }

        [JsonPropertyName("url")]
        public Uri? Url { get; set; }
    }
}