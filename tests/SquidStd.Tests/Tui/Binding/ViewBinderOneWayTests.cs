using System.ComponentModel;
using SquidStd.Tui.Binding;

namespace SquidStd.Tests.Tui.Binding;

public class ViewBinderOneWayTests
{
    private sealed class FakeViewModel : INotifyPropertyChanged
    {
        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    [Fact]
    public void OneWay_AppliesInitialValueAndUpdates()
    {
        var vm = new FakeViewModel { Title = "first" };
        var applied = string.Empty;
        var binder = new ViewBinder();

        binder.OneWay(vm, nameof(FakeViewModel.Title), () => applied = vm.Title);

        Assert.Equal("first", applied);

        vm.Title = "second";
        Assert.Equal("second", applied);
    }

    [Fact]
    public void Dispose_StopsUpdates()
    {
        var vm = new FakeViewModel();
        var applied = string.Empty;
        var binder = new ViewBinder();
        binder.OneWay(vm, nameof(FakeViewModel.Title), () => applied = vm.Title);

        binder.Dispose();
        vm.Title = "after-dispose";

        Assert.Equal(string.Empty, applied);
    }

    [Fact]
    public void OneWay_UsesMarshal()
    {
        var vm = new FakeViewModel();
        var marshalled = 0;
        var binder = new ViewBinder(action => { marshalled++; action(); });
        binder.OneWay(vm, nameof(FakeViewModel.Title), () => { });

        vm.Title = "x";

        Assert.Equal(1, marshalled); // change marshalled once; the initial apply is NOT marshalled
    }
}
