using SquidStd.Tui.Interfaces;
using Terminal.Gui.ViewBase;

namespace SquidStd.Tui.Hosting;

/// <summary>
/// Shows views as full-size children of a shell container supplied by <see cref="TuiApplicationHost" />.
/// The container is set before the first navigation, so <see cref="Show" /> never no-ops on a null top view
/// (Terminal.Gui 2.4.16 has no <c>Application.Top</c>; the running top view is null until the loop starts).
/// </summary>
public sealed class TerminalGuiViewHost : ITuiViewHost
{
    /// <summary>The shell the navigator's screens are added to. Set by the application host before running.</summary>
    public View? Container { get; set; }

    /// <summary>Adds <paramref name="view" /> as a full-size child of <see cref="Container" /> and gives it focus.</summary>
    public void Show(object view)
    {
        if (Container is not null && view is View concrete)
        {
            concrete.Width = Dim.Fill();
            concrete.Height = Dim.Fill();
            Container.Add(concrete);
            concrete.SetFocus();
        }
    }

    /// <summary>Removes <paramref name="view" /> from <see cref="Container" /> and disposes it.</summary>
    public void Remove(object view)
    {
        if (Container is not null && view is View concrete)
        {
            Container.Remove(concrete);
            concrete.Dispose();
        }
    }
}
