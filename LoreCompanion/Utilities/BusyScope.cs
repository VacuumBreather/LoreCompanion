namespace LoreCompanion.Utilities
{
    public sealed class BusyScope(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _onDispose();
            }
        }
    }
}