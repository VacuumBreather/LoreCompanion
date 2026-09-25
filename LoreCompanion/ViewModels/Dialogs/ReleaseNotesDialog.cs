namespace LoreCompanion.ViewModels.Dialogs
{
    public sealed class ReleaseNotesDialog(Version version) : QueryDialog(
        "Publish Database",
        $"Enter release notes - Current version: {version}",
        DialogResults.Ok,
        DialogResult.Ok)
    {
        public string ReleaseNotes
        {
            get;
            set => Set(ref field, value);
        } = string.Empty;

        public bool IsMinorVersionRelease
        {
            get;
            set => Set(ref field, value);
        }
    }
}