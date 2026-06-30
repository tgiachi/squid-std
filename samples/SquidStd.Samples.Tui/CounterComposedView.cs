using SquidStd.Tui;
using SquidStd.Tui.Dsl;

namespace SquidStd.Samples.Tui;

public sealed class CounterComposedView : TuiComposedView<CounterViewModel>
{
    protected override TuiNode<CounterViewModel> Compose()
        => Ui.VStack(
            Ui.Label(x => x.Title),
            Ui.Label(x => x.Value),
            Ui.Button("+1", x => x.IncrementCommand)
        );
}
