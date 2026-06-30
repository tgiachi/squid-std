using System.ComponentModel;
using SquidStd.Tui.Binding;
using SquidStd.Tui.Types.Tui;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace SquidStd.Tui.Dsl;

/// <summary>
/// Turns a <see cref="TuiNode{TViewModel}" /> tree into a Terminal.Gui view graph, wiring each
/// node's binding through the supplied <see cref="ViewBinder" />.
/// </summary>
public sealed class TuiNodeMaterializer
{
    /// <summary>
    /// Materialises <paramref name="node" /> against <paramref name="viewModel" />, registering
    /// bindings on <paramref name="binder" />, and returns the produced view.
    /// </summary>
    public View Materialize<TViewModel>(TuiNode<TViewModel> node, TViewModel viewModel, ViewBinder binder)
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(binder);

        switch (node)
        {
            case LabelNode<TViewModel> labelNode:
                var label = new Label();
                binder.OneWayText(viewModel, labelNode.Text, label);

                return label;

            case TextFieldNode<TViewModel> fieldNode:
                var field = new TextField();

                if (fieldNode.Mode == BindMode.TwoWay)
                {
                    binder.TwoWay(viewModel, fieldNode.Text, field);
                }
                else
                {
                    binder.OneWay(viewModel, fieldNode.Text, field, f => f.Text);
                }

                return field;

            case ButtonNode<TViewModel> buttonNode:
                var button = new Button { Text = buttonNode.Caption };
                binder.Command(button, buttonNode.Command.Compile()(viewModel));

                return button;

            case StackNode<TViewModel> stackNode:
                return MaterializeStack(stackNode, viewModel, binder);

            default:
                throw new NotSupportedException($"Unsupported node type '{node.GetType().Name}'.");
        }
    }

    private View MaterializeStack<TViewModel>(StackNode<TViewModel> stack, TViewModel viewModel, ViewBinder binder)
        where TViewModel : INotifyPropertyChanged
    {
        var container = new View
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        View? previous = null;

        foreach (var childNode in stack.Children)
        {
            var child = Materialize(childNode, viewModel, binder);

            if (stack.Orientation == StackOrientation.Vertical)
            {
                child.X = 0;
                child.Y = previous is null ? 0 : Pos.Bottom(previous);
                child.Width = Dim.Fill();
            }
            else
            {
                child.X = previous is null ? 0 : Pos.Right(previous);
                child.Y = 0;
                child.Height = Dim.Fill();
            }

            container.Add(child);
            previous = child;
        }

        return container;
    }
}
