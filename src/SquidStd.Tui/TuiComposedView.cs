using SquidStd.Tui.Binding;
using SquidStd.Tui.Dsl;

namespace SquidStd.Tui;

/// <summary>
/// Declarative view base: implement <see cref="Compose" /> to return a node tree built with <see cref="Ui" />.
/// The tree is materialised into widgets and bindings during initialisation. For imperative views use
/// <see cref="TuiView{TViewModel}" /> directly.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel type for this view.</typeparam>
public abstract class TuiComposedView<TViewModel> : TuiView<TViewModel>
    where TViewModel : TuiViewModel
{
    private readonly TuiNodeMaterializer _materializer = new();

    /// <summary>Factory for building the node tree from typed lambdas.</summary>
    protected UiFactory<TViewModel> Ui { get; } = new();

    /// <summary>Returns the declarative node tree for this view.</summary>
    protected abstract TuiNode<TViewModel> Compose();

    protected sealed override void BuildLayout() { }

    protected sealed override void Bind(ViewBinder binder) { }

    protected override void OnInitialize(ViewBinder binder)
    {
        var root = _materializer.Materialize(Compose(), ViewModel, binder);
        Add(root);
    }
}
