using System.Linq.Expressions;
using System.Windows.Input;
using SquidStd.Tui.Types.Tui;

namespace SquidStd.Tui.Dsl;

/// <summary>
/// Builds DSL nodes from typed lambdas. The ViewModel type is fixed by the factory instance so the
/// member-expression lambdas (<c>x =&gt; x.Property</c>) infer without annotations.
/// </summary>
/// <typeparam name="TViewModel">The ViewModel the built nodes bind to.</typeparam>
public sealed class UiFactory<TViewModel>
{
    /// <summary>A label one-way bound to a string property.</summary>
    public LabelNode<TViewModel> Label(Expression<Func<TViewModel, string>> text)
        => new(text);

    /// <summary>A text field bound to a string property (two-way by default).</summary>
    public TextFieldNode<TViewModel> TextField(Expression<Func<TViewModel, string>> text, BindMode mode = BindMode.TwoWay)
        => new(text, mode);

    /// <summary>A button that runs the command resolved from the ViewModel.</summary>
    public ButtonNode<TViewModel> Button(string caption, Expression<Func<TViewModel, ICommand>> command)
        => new(caption, command);

    /// <summary>A vertical stack of child nodes.</summary>
    public StackNode<TViewModel> VStack(params TuiNode<TViewModel>[] children)
        => new(StackOrientation.Vertical, children);

    /// <summary>A horizontal stack of child nodes.</summary>
    public StackNode<TViewModel> HStack(params TuiNode<TViewModel>[] children)
        => new(StackOrientation.Horizontal, children);
}
