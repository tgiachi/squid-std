using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SquidStd.Tui;

namespace SquidStd.Samples.Tui;

public sealed partial class CounterViewModel : TuiViewModel
{
    [ObservableProperty] private string _title = "SquidStd.Tui — Counter";

    [ObservableProperty] private string _value = "0";

    [RelayCommand]
    private void Increment()
        => Value = (int.Parse(Value) + 1).ToString();
}
