namespace LoreCompanion.ViewModels.Dialogs
{
    public class VideoPlayerDialog(string title, string content) : InformationDialog(title, content)
    {
        protected override Task OnDialogClosed(CancellationToken cancellationToken)
        {
            Content = "about:blank";

            return base.OnDialogClosed(cancellationToken);
        }
    }
}