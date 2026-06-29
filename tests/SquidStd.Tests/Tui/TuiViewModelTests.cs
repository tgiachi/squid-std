using CommunityToolkit.Mvvm.ComponentModel;
using SquidStd.Tui;

namespace SquidStd.Tests.Tui;

public partial class TuiViewModelTests
{
    private sealed partial class SampleViewModel : TuiViewModel
    {
        [ObservableProperty]
        private string _title = string.Empty;

        public int Activated { get; private set; }

        public override ValueTask OnActivatedAsync()
        {
            Activated++;

            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public void ObservableProperty_RaisesPropertyChanged()
    {
        var vm = new SampleViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Title = "hello";

        Assert.Contains(nameof(SampleViewModel.Title), raised);
    }

    [Fact]
    public async Task OnActivatedAsync_IsOverridable()
    {
        var vm = new SampleViewModel();

        await vm.OnActivatedAsync();

        Assert.Equal(1, vm.Activated);
    }

    [Fact]
    public async Task OnDeactivatedAsync_DefaultsToNoOp()
    {
        var vm = new SampleViewModel();

        await vm.OnDeactivatedAsync(); // does not throw
    }
}
