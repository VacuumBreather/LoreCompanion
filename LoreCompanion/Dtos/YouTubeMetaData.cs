using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace LoreCompanion.Dtos
{
    [PublicAPI]
    public sealed record YouTubeMetaData(
        [property: JsonPropertyName("title")]
        string Title,
        [property: JsonPropertyName("author_name")]
        string AuthorName,
        [property: JsonPropertyName("author_url")]
        string AuthorUrl,
        [property: JsonPropertyName("thumbnail_url")]
        string ThumbnailUrl);
}