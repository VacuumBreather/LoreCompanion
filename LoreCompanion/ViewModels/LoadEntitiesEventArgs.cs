namespace LoreCompanion.ViewModels
{
    public class LoadEntitiesEventArgs : EventArgs
    {
        public bool ForceLoad { get; init; }

        public CancellationToken CancellationToken { get; init; }
    }
}