namespace LoreCompanion.ViewModels.Dialogs
{
    public class VideoPlayerDialog(string title, string videoKey, TimeSpan timestamp = default) : DialogScreen
    {
        public string Title { get; } = title;

        public string VideoKey { get; } = videoKey;

        public TimeSpan Timestamp { get; } = timestamp;
    }
}