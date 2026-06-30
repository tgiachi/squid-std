using DryIoc;
using SquidStd.Samples.Tui;
using SquidStd.Tui.Extensions;
using SquidStd.Tui.Hosting;

var container = new Container();
container.RegisterTui();
container.RegisterView<CounterComposedView, CounterViewModel>();

await container.Resolve<TuiApplicationHost>().RunAsync<CounterViewModel>();
