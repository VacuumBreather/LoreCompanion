using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace LoreCompanion.Dtos
{
    [PublicAPI]
    public sealed record DatabaseManifest(
        [property: JsonPropertyName("version")]
        Version? Version,
        [property: JsonPropertyName("sha256")]
        string? Sha256,
        [property: JsonPropertyName("required_app_version")]
        Version? RequiredAppVersion,
        [property: JsonPropertyName("downloadUrl")]
        Uri? DownloadUrl);
}