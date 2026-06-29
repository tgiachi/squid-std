using System.ComponentModel;
using SquidStd.Tui.Internal;

namespace SquidStd.Tui.Binding;

/// <summary>
/// Wires ViewModel (<see cref="INotifyPropertyChanged" />) properties and commands to view targets, and
/// owns the lifetime of every subscription it creates. Dispose to release them all.
/// </summary>
public sealed partial class ViewBinder : IDisposable
{
    private readonly List<IDisposable> _subscriptions = new();
    private readonly Action<Action> _marshal;

    /// <param name="marshal">
    /// Runs an update on the UI thread. Defaults to running inline; the host passes a delegate over
    /// <c>Application.Invoke</c> so updates raised from background threads reach the Terminal.Gui loop.
    /// </param>
    public ViewBinder(Action<Action>? marshal = null)
    {
        _marshal = marshal ?? (action => action());
    }

    /// <summary>Applies <paramref name="apply" /> now and whenever <paramref name="propertyName" /> changes.</summary>
    public void OneWay(INotifyPropertyChanged source, string propertyName, Action apply)
    {
        apply();

        void Handler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is null || string.Equals(e.PropertyName, propertyName, StringComparison.Ordinal))
            {
                _marshal(apply);
            }
        }

        source.PropertyChanged += Handler;
        _subscriptions.Add(new Unsubscriber(() => source.PropertyChanged -= Handler));
    }

    private void Track(IDisposable subscription)
    {
        _subscriptions.Add(subscription);
    }

    public void Dispose()
    {
        for (var i = 0; i < _subscriptions.Count; i++)
        {
            _subscriptions[i].Dispose();
        }

        _subscriptions.Clear();
    }
}
