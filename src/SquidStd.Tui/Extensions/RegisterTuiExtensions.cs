using DryIoc;
using SquidStd.Tui.Hosting;
using SquidStd.Tui.Interfaces;
using SquidStd.Tui.Internal;
using SquidStd.Tui.Navigation;

namespace SquidStd.Tui.Extensions;

/// <summary>DryIoc registration helpers for the TUI module.</summary>
public static class RegisterTuiExtensions
{
    extension(IContainer container)
    {
        /// <summary>Registers the navigator, view registry, view host and application host.</summary>
        public IContainer RegisterTui()
        {
            container.Register<TuiViewRegistry>(Reuse.Singleton);
            container.Register<TerminalGuiViewHost>(Reuse.Singleton);
            container.RegisterMapping<ITuiViewHost, TerminalGuiViewHost>();
            container.Register<ITuiNavigator, TuiNavigator>(Reuse.Singleton);
            container.Register<TuiApplicationHost>(Reuse.Singleton);

            return container;
        }

        /// <summary>Registers a View/ViewModel pair and records the mapping for navigation.</summary>
        public IContainer RegisterView<TView, TViewModel>()
            where TView : class, ITuiView
            where TViewModel : TuiViewModel
        {
            // Views derive from Window (IDisposable). DryIoc refuses disposable transients by default;
            // allow it without tracking — the view host disposes each view on Remove, so the container
            // must not also dispose it (which would double-dispose).
            container.Register<TView>(Reuse.Transient, setup: Setup.With(allowDisposableTransient: true));
            container.Register<TViewModel>(Reuse.Transient);
            container.Resolve<TuiViewRegistry>().Map(typeof(TViewModel), typeof(TView));

            return container;
        }
    }
}
