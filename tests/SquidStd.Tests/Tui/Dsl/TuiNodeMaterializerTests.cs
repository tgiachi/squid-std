using CommunityToolkit.Mvvm.ComponentModel;
using SquidStd.Tui.Binding;
using SquidStd.Tui.Dsl;
using Terminal.Gui.Views;

namespace SquidStd.Tests.Tui.Dsl;

public partial class TuiNodeMaterializerTests
{
    [Fact]
    public void Materialize_VStack_ProducesChildrenAndAppliesOneWay()
    {
        var ui = new UiFactory<SampleViewModel>();
        var vm = new SampleViewModel();
        var binder = new ViewBinder();
        var materializer = new TuiNodeMaterializer();

        var root = materializer.Materialize(ui.VStack(ui.Label(x => x.Title)), vm, binder);

        var label = Assert.IsType<Label>(Assert.Single(root.SubViews));
        Assert.Equal("start", label.Text);

        vm.Title = "changed";
        Assert.Equal("changed", label.Text);
    }

    [Fact]
    public void Materialize_TextField_TwoWay_WritesBackToViewModel()
    {
        var ui = new UiFactory<SampleViewModel>();
        var vm = new SampleViewModel();
        var binder = new ViewBinder();
        var materializer = new TuiNodeMaterializer();

        var field = Assert.IsType<TextField>(materializer.Materialize(ui.TextField(x => x.Name), vm, binder));

        vm.Name = "from-vm";
        Assert.Equal("from-vm", field.Text);
    }

    private sealed partial class SampleViewModel : ObservableObject
    {
        [ObservableProperty] private string _title = "start";

        [ObservableProperty] private string _name = string.Empty;
    }
}
