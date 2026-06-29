using SquidStd.Tui;
using SquidStd.Tui.Binding;
using Terminal.Gui.Views;

namespace SquidStd.Samples.Tui;

public sealed class CounterView : TuiView<CounterViewModel>
{
    private Label _value = null!;
    private Button _increment = null!;

    protected override void BuildLayout()
    {
        _value = new Label { X = 1, Y = 1 };
        _increment = new Button { X = 1, Y = 3, Text = "_Increment" };
        Add(_value, _increment);
    }

    protected override void Bind(ViewBinder binder)
    {
        binder.OneWayTitle(ViewModel, x => x.Title, this);
        binder.OneWayText(ViewModel, x => x.Value, _value);
        binder.Command(_increment, ViewModel.IncrementCommand);
    }
}
