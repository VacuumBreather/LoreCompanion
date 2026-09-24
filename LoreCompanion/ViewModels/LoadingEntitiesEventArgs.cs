namespace LoreCompanion.ViewModels
{
    public class LoadingEntitiesEventArgs : EventArgs
    {
        public bool ForceLoad { get; init; }

        public CancellationToken CancellationToken { get; init; }
    }
}