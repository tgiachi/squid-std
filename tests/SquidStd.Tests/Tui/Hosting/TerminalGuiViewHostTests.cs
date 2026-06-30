using SquidStd.Tui.Hosting;
using Terminal.Gui.ViewBase;

namespace SquidStd.Tests.Tui.Hosting;

public class TerminalGuiViewHostTests
{
    [Fact]
    public void Show_AddsViewToContainer_AndRemoveDetachesIt()
    {
        var host = new TerminalGuiViewHost();
        var shell = new View();
        host.Container = shell;

        var screen = new View();
        host.Show(screen);

        Assert.Contains(screen, shell.SubViews);

        host.Remove(screen);

        Assert.DoesNotContain(screen, shell.SubViews);
    }

    [Fact]
    public void Show_WithNullContainer_DoesNotThrow()
    {
        var host = new TerminalGuiViewHost();

        host.Show(new View()); // no container set -> no-op, must not throw
    }
}
