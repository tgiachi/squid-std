namespace SquidStd.Tui.Internal;

/// <summary>Single-threaded reentrancy flag used to stop two-way binding write-back loops.</summary>
internal sealed class ReentryGuard
{
    public bool IsBusy { get; private set; }

    public IDisposable Enter()
    {
        IsBusy = true;

        return new Scope(this);
    }

    private sealed class Scope : IDisposable
    {
        private readonly ReentryGuard _owner;

        public Scope(ReentryGuard owner)
        {
            _owner = owner;
        }

        public void Dispose()
            => _owner.IsBusy = false;
    }
}
