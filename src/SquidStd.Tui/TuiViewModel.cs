using CommunityToolkit.Mvvm.ComponentModel;
using SquidStd.Tui.Interfaces;

namespace SquidStd.Tui;

/// <summary>
/// Base class for TUI ViewModels. Extends <see cref="ObservableObject" /> (so CommunityToolkit's
/// <c>[ObservableProperty]</c>/<c>[RelayCommand]</c> generators apply) and adds navigation access plus
/// activation lifecycle hooks the navigator invokes.
/// </summary>
public abstract class TuiViewModel : ObservableObject
{
    /// <summary>The navigator, assigned by the navigation system when the screen is pushed.</summary>
    public ITuiNavigator Navigator { get; internal set; } = null!;

    /// <summary>Invoked after the screen becomes active. Default is a no-op.</summary>
    public virtual ValueTask OnActivatedAsync()
        => ValueTask.CompletedTask;

    /// <summary>Invoked after the screen is removed or hidden. Default is a no-op.</summary>
    public virtual ValueTask OnDeactivatedAsync()
        => ValueTask.CompletedTask;
}
