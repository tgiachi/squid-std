<h1 align="center">SquidStd.Tui</h1>

MVVM for terminal apps, built on **Terminal.Gui v2** and **CommunityToolkit.Mvvm**. Observable
ViewModels, a View↔ViewModel binder (fluent + opt-in convention), ViewModel-first navigation, and DryIoc
wiring.

## Install

```bash
dotnet add package SquidStd.Tui
```

## Usage

```csharp
public sealed partial class CounterViewModel : TuiViewModel
{
    [ObservableProperty] private string _title = "Counter";
    [ObservableProperty] private string _value = "0";

    [RelayCommand]
    private void Increment() => Value = (int.Parse(Value) + 1).ToString();
}

public sealed class CounterView : TuiView<CounterViewModel>
{
    private Label _value = null!;
    private Button _inc = null!;

    protected override void BuildLayout()
    {
        _value = new Label { X = 1, Y = 1 };
        _inc = new Button { X = 1, Y = 3, Text = "+1" };
        Add(_value, _inc);
    }

    protected override void Bind(ViewBinder b)
    {
        b.OneWayTitle(ViewModel, x => x.Title, this);
        b.OneWayText(ViewModel, x => x.Value, _value);
        b.Command(_inc, ViewModel.IncrementCommand);
    }
}

// boot
var container = new Container().RegisterTui();
container.RegisterView<CounterView, CounterViewModel>();
await container.Resolve<TuiApplicationHost>().RunAsync<CounterViewModel>();
```

## Declarative UI (DSL)

Instead of imperative `BuildLayout` + `Bind`, derive from `TuiComposedView<TViewModel>` and return a node
tree from `Compose()`. One node = widget + binding; direction is inferred (Label one-way, TextField
two-way, Button command) and bindings are typed lambdas - no magic strings.

```csharp
public sealed class CounterView : TuiComposedView<CounterViewModel>
{
    protected override TuiNode<CounterViewModel> Compose() =>
        Ui.VStack(
            Ui.Label(x => x.Title),
            Ui.Label(x => x.Value),
            Ui.Button("+1", x => x.IncrementCommand));
}
```

`Ui.VStack`/`Ui.HStack` arrange children automatically; `Ui.TextField(x => x.Name)` is two-way by default
(pass `BindMode.OneWay` to override).

## Key types

| Type                                  | Purpose                                                                       |
|---------------------------------------|-------------------------------------------------------------------------------|
| `TuiViewModel`                        | ViewModel base (ObservableObject + activation hooks + `Navigator`).           |
| `TuiView<TViewModel>`                 | View base: a Terminal.Gui `Window` with a typed ViewModel and a binder.       |
| `ViewBinder`                          | One-way / two-way / command binding; fluent typed + `AutoBind` by convention. |
| `ITuiNavigator`                       | ViewModel-first stack navigation (`NavigateToAsync`, `BackAsync`).            |
| `TuiApplicationHost`                  | Boots the Terminal.Gui loop with a root ViewModel.                            |
| `RegisterTui()` / `RegisterView<,>()` | DryIoc registration.                                                          |

## Related

- Article: [Terminal UI](https://tgiachi.github.io/squid-std/articles/tui.html)
- Tutorial: [Terminal UI (MVVM)](https://tgiachi.github.io/squid-std/tutorials/tui.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
