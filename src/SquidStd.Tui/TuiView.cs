using SquidStd.Tui.Binding;
using SquidStd.Tui.Interfaces;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace SquidStd.Tui;

/// <summary>
/// Base class for views: a Terminal.Gui <see cref="Window" /> that owns a typed ViewModel and a
/// <see cref="ViewBinder" />. Implement <see cref="BuildLayout" /> to create widgets and
/// <see cref="Bind" /> to declare bindings; both run once during <see cref="ITuiView.Initialize" />.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel type for this view.</typeparam>
public abstract class TuiView<TViewModel> : Window, ITuiView
    where TViewModel : TuiViewModel
{
    private readonly ViewBinder _binder;

    /// <summary>The bound ViewModel. Set by the navigator before <see cref="ITuiView.Initialize" />.</summary>
    protected TViewModel ViewModel { get; private set; } = null!;

    protected TuiView()
    {
        _binder = new(Application.Invoke);
    }

    void ITuiView.Bind(object viewModel)
        => ViewModel = (TViewModel)viewModel;

    void ITuiView.Initialize()
        => OnInitialize(_binder);

    /// <summary>
    /// Builds the view. The default runs <see cref="BuildLayout" /> then <see cref="Bind" />; the
    /// declarative base overrides this to materialise a node tree instead.
    /// </summary>
    protected virtual void OnInitialize(ViewBinder binder)
    {
        BuildLayout();
        Bind(binder);
    }

    /// <summary>Creates the Terminal.Gui widgets for this view.</summary>
    protected abstract void BuildLayout();

    /// <summary>Declares the bindings between <see cref="ViewModel" /> and the widgets.</summary>
    protected abstract void Bind(ViewBinder binder);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _binder.Dispose();
        }

        base.Dispose(disposing);
    }
}
