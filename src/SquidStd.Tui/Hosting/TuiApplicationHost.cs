using SquidStd.Tui.Interfaces;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace SquidStd.Tui.Hosting;

/// <summary>
/// Boots a Terminal.Gui application: initialises the driver, creates a full-screen shell, navigates to the
/// root ViewModel (whose view is added to the shell), runs the event loop, then shuts the driver down.
/// </summary>
public sealed class TuiApplicationHost
{
    private readonly ITuiNavigator _navigator;
    private readonly TerminalGuiViewHost _viewHost;

    public TuiApplicationHost(ITuiNavigator navigator, TerminalGuiViewHost viewHost)
    {
        _navigator = navigator;
        _viewHost = viewHost;
    }

    /// <summary>Runs the application with <typeparamref name="TRootViewModel" /> as the first screen.</summary>
    public async Task RunAsync<TRootViewModel>()
        where TRootViewModel : TuiViewModel
    {
        Application.Init();

        try
        {
            var shell = new Window
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.None
            };

            _viewHost.Container = shell;

            await _navigator.NavigateToAsync<TRootViewModel>();

            Application.Run(shell);
        }
        finally
        {
            _viewHost.Container = null;
            Application.Shutdown();
        }
    }
}
