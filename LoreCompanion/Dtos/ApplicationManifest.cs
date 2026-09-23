using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace LoreCompanion.Dtos
{
    [PublicAPI]
    public sealed record ApplicationManifest(
        [property: JsonPropertyName("version")]
        Version? Version,
        [property: JsonPropertyName("url")]
        Uri? Url);
}