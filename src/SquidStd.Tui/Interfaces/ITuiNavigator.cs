using SquidStd.Tui;

namespace SquidStd.Tui.Interfaces;

/// <summary>ViewModel-first navigation over a stack of screens.</summary>
public interface ITuiNavigator
{
    /// <summary>Number of screens currently on the navigation stack.</summary>
    int Depth { get; }

    /// <summary>Resolves <typeparamref name="TViewModel" /> and its view and pushes it onto the stack.</summary>
    Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : TuiViewModel;

    /// <summary>Pops the current screen and reactivates the one beneath it.</summary>
    Task BackAsync(CancellationToken cancellationToken = default);
}
