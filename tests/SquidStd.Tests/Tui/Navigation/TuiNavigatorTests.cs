using DryIoc;
using SquidStd.Tui;
using SquidStd.Tui.Interfaces;
using SquidStd.Tui.Internal;
using SquidStd.Tui.Navigation;

namespace SquidStd.Tests.Tui.Navigation;

public class TuiNavigatorTests
{
    private sealed class HomeViewModel : TuiViewModel
    {
    }

    private sealed class DetailViewModel : TuiViewModel
    {
    }

    // Non-generic fake view that records the ViewModel it was given and that it was initialised.
    private sealed class FakeView : ITuiView
    {
        public object? BoundViewModel { get; private set; }
        public bool Initialized { get; private set; }

        public void Bind(object viewModel)
        {
            BoundViewModel = viewModel;
        }

        public void Initialize()
        {
            Initialized = true;
        }
    }

    private sealed class FakeViewHost : ITuiViewHost
    {
        public List<object> Shown { get; } = new();
        public List<object> Removed { get; } = new();

        public void Show(object view)
        {
            Shown.Add(view);
        }

        public void Remove(object view)
        {
            Removed.Add(view);
        }
    }

    private static (TuiNavigator Navigator, FakeViewHost Host) Build()
    {
        var container = new Container();
        container.Register<HomeViewModel>(Reuse.Transient);
        container.Register<DetailViewModel>(Reuse.Transient);
        container.Register<FakeView>(Reuse.Transient);

        var registry = new TuiViewRegistry();
        registry.Map(typeof(HomeViewModel), typeof(FakeView));
        registry.Map(typeof(DetailViewModel), typeof(FakeView));

        var host = new FakeViewHost();
        var navigator = new TuiNavigator(container, registry, host);

        return (navigator, host);
    }

    [Fact]
    public async Task NavigateTo_PushesViewModelBoundView_AndShowsIt()
    {
        var (navigator, host) = Build();

        await navigator.NavigateToAsync<HomeViewModel>();

        Assert.Equal(1, navigator.Depth);
        Assert.Single(host.Shown);
        var view = Assert.IsType<FakeView>(host.Shown[0]);
        Assert.True(view.Initialized);
        Assert.IsType<HomeViewModel>(view.BoundViewModel);
        Assert.Same(navigator, ((HomeViewModel)view.BoundViewModel!).Navigator);
    }

    [Fact]
    public async Task Back_PopsCurrentAndRemovesItsView()
    {
        var (navigator, host) = Build();
        await navigator.NavigateToAsync<HomeViewModel>();
        await navigator.NavigateToAsync<DetailViewModel>();

        Assert.Equal(2, navigator.Depth);

        await navigator.BackAsync();

        Assert.Equal(1, navigator.Depth);
        Assert.Single(host.Removed);
    }

    [Fact]
    public async Task Back_OnLastScreen_DoesNothing()
    {
        var (navigator, _) = Build();
        await navigator.NavigateToAsync<HomeViewModel>();

        await navigator.BackAsync();

        Assert.Equal(1, navigator.Depth); // refuses to pop the root
    }

    private sealed class TrackingHomeViewModel : TuiViewModel
    {
        public int Activated { get; private set; }
        public int Deactivated { get; private set; }

        public override ValueTask OnActivatedAsync()
        {
            Activated++;
            return ValueTask.CompletedTask;
        }

        public override ValueTask OnDeactivatedAsync()
        {
            Deactivated++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TrackingDetailViewModel : TuiViewModel
    {
        public int Activated { get; private set; }
        public int Deactivated { get; private set; }

        public override ValueTask OnActivatedAsync()
        {
            Activated++;
            return ValueTask.CompletedTask;
        }

        public override ValueTask OnDeactivatedAsync()
        {
            Deactivated++;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task ForwardThenBack_PairsActivationHooks_NoDoubleActivateWithoutDeactivate()
    {
        var container = new Container();
        container.Register<TrackingHomeViewModel>(Reuse.Singleton);
        container.Register<TrackingDetailViewModel>(Reuse.Singleton);
        container.Register<FakeView>(Reuse.Transient);

        var registry = new TuiViewRegistry();
        registry.Map(typeof(TrackingHomeViewModel), typeof(FakeView));
        registry.Map(typeof(TrackingDetailViewModel), typeof(FakeView));

        var navigator = new TuiNavigator(container, registry, new FakeViewHost());
        var home = container.Resolve<TrackingHomeViewModel>();
        var detail = container.Resolve<TrackingDetailViewModel>();

        await navigator.NavigateToAsync<TrackingHomeViewModel>();   // home: A1 D0
        Assert.Equal(1, home.Activated);
        Assert.Equal(0, home.Deactivated);

        await navigator.NavigateToAsync<TrackingDetailViewModel>(); // home deactivated; detail A1
        Assert.Equal(1, home.Activated);
        Assert.Equal(1, home.Deactivated);
        Assert.Equal(1, detail.Activated);

        await navigator.BackAsync();                                 // detail D1; home reactivated
        Assert.Equal(1, detail.Deactivated);
        Assert.Equal(2, home.Activated);
        Assert.Equal(1, home.Deactivated); // still 1 — every activate is now paired with a prior deactivate
    }
}
