namespace SquidStd.Tui.Internal;

/// <summary>An <see cref="IDisposable" /> that runs a single unsubscribe action once.</summary>
internal sealed class Unsubscriber : IDisposable
{
    private Action? _unsubscribe;

    public Unsubscriber(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    public void Dispose()
    {
        var action = _unsubscribe;
        _unsubscribe = null;
        action?.Invoke();
    }
}
