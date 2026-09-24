namespace LoreCompanion.Utilities
{
    public static class YouTubeHelper
    {
        public const string VideoUrlFormatString = "https://www.youtube.com/watch?v={0}";

        public const string VideoUrlFormatStringWithTime = "https://www.youtube.com/watch?v={0}&t={1}s";

        public const string ThumbnailUrlFormatString = "https://img.youtube.com/vi/{0}/mqdefault.jpg";

        public const string MetadataUrlFormatString =
            "https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={0}&format=json";
    }
}