using DryIoc;
using SquidStd.Samples.Tui;
using SquidStd.Tui.Extensions;
using SquidStd.Tui.Hosting;

var container = new Container();
container.RegisterTui();
container.RegisterView<CounterView, CounterViewModel>();

await container.Resolve<TuiApplicationHost>().RunAsync<CounterViewModel>();
