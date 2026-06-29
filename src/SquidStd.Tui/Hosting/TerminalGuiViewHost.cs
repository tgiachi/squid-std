using SquidStd.Tui.Interfaces;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

namespace SquidStd.Tui.Hosting;

/// <summary>Shows views as top-level windows on the running Terminal.Gui application.</summary>
public sealed class TerminalGuiViewHost : ITuiViewHost
{
    public void Show(object view)
    {
        if (view is View concrete)
        {
            Application.TopRunnableView?.Add(concrete);
            concrete.SetFocus();
        }
    }

    public void Remove(object view)
    {
        if (view is View concrete)
        {
            Application.TopRunnableView?.Remove(concrete);
            concrete.Dispose();
        }
    }
}
