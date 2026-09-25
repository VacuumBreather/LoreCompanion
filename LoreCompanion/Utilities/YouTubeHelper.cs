namespace LoreCompanion.Utilities
{
    public static class YouTubeHelper
    {
        public const string Referer = "https://vacuumbreather.de/lorecompanion/";

        public const string ThumbnailUrlFormatString = "https://img.youtube.com/vi/{0}/mqdefault.jpg";

        public const string MetadataUrlFormatString =
            "https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={0}&format=json";

        private const string Origin = "https://vacuumbreather.de";

        public static string BuildEmbedUrl(string videoKey, TimeSpan timestamp = default)
        {
            if (string.IsNullOrWhiteSpace(videoKey))
            {
                return string.Empty;
            }

            var seconds = Math.Max(0, (int)timestamp.TotalSeconds);

            return
                $"https://www.youtube.com/embed/{Uri.EscapeDataString(videoKey.Trim())}?autoplay=1&enablejsapi=1&start={seconds}&origin={Uri.EscapeDataString(Origin)}";
        }
    }
}