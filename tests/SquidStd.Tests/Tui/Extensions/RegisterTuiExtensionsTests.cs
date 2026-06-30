using DryIoc;
using SquidStd.Tui;
using SquidStd.Tui.Extensions;
using SquidStd.Tui.Hosting;
using SquidStd.Tui.Interfaces;
using SquidStd.Tui.Internal;

namespace SquidStd.Tests.Tui.Extensions;

public class RegisterTuiExtensionsTests
{
    private sealed class HomeViewModel : TuiViewModel
    {
    }

    private sealed class HomeView : ITuiView
    {
        public void Bind(object viewModel)
        {
        }

        public void Initialize()
        {
        }
    }

    // Real views derive from Window (IDisposable); this fake reproduces that shape.
    private sealed class DisposableView : ITuiView, IDisposable
    {
        public void Bind(object viewModel)
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }

    [Fact]
    public void RegisterTui_RegistersNavigatorAndRegistry()
    {
        var container = new Container();

        container.RegisterTui();

        Assert.NotNull(container.Resolve<ITuiNavigator>());
        Assert.NotNull(container.Resolve<TuiViewRegistry>());
        Assert.NotNull(container.Resolve<ITuiViewHost>());
        Assert.Same(container.Resolve<TerminalGuiViewHost>(), container.Resolve<ITuiViewHost>());
    }

    [Fact]
    public void RegisterView_MapsAndResolvesBothTypes()
    {
        var container = new Container();
        container.RegisterTui();

        container.RegisterView<HomeView, HomeViewModel>();

        var registry = container.Resolve<TuiViewRegistry>();
        Assert.Equal(typeof(HomeView), registry.ViewTypeFor(typeof(HomeViewModel)));
        Assert.NotNull(container.Resolve<HomeViewModel>());
        Assert.NotNull(container.Resolve<HomeView>());
    }

    [Fact]
    public void RegisterView_AllowsDisposableTransientView()
    {
        var container = new Container();
        container.RegisterTui();

        // Real views are IDisposable (Window); DryIoc throws on disposable transients unless allowed.
        container.RegisterView<DisposableView, HomeViewModel>();

        Assert.NotNull(container.Resolve<DisposableView>());
    }
}
