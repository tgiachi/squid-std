using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SquidStd.Tui.Binding;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace SquidStd.Tests.Tui.Binding;

public partial class ViewBinderAutoBindTests
{
    private sealed partial class AutoBindViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        // Wrong type on purpose — used by the "wrong type" skip-path test.
        public int Count { get; } = 42;

        private bool _canSave;

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save() { }

        private bool CanSave() => _canSave;

        public void SetCanSave(bool value)
        {
            _canSave = value;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    // -------------------------------------------------------------------
    // Label one-way
    // -------------------------------------------------------------------

    [Fact]
    public void AutoBind_Label_AppliesInitialValue()
    {
        var vm = new AutoBindViewModel { Title = "Hello" };
        var parent = new View();
        var label = new Label { Id = "TitleLabel" };
        parent.Add(label);

        var binder = new ViewBinder();
        binder.AutoBind(parent, vm);

        Assert.Equal("Hello", label.Text);
    }

    [Fact]
    public void AutoBind_Label_UpdatesWhenVmPropertyChanges()
    {
        var vm = new AutoBindViewModel { Title = "initial" };
        var parent = new View();
        var label = new Label { Id = "TitleLabel" };
        parent.Add(label);

        var binder = new ViewBinder();
        binder.AutoBind(parent, vm);

        vm.Title = "updated";

        Assert.Equal("updated", label.Text);
    }

    // -------------------------------------------------------------------
    // TextField two-way (vm→field direction; writeback omitted headless)
    // -------------------------------------------------------------------

    [Fact]
    public void AutoBind_TextField_AppliesInitialValue()
    {
        var vm = new AutoBindViewModel { Name = "squid" };
        var parent = new View();
        var field = new TextField { Id = "NameField" };
        parent.Add(field);

        var binder = new ViewBinder();
        binder.AutoBind(parent, vm);

        Assert.Equal("squid", field.Text);
    }

    [Fact]
    public void AutoBind_TextField_UpdatesFieldWhenVmPropertyChanges()
    {
        var vm = new AutoBindViewModel { Name = "before" };
        var parent = new View();
        var field = new TextField { Id = "NameField" };
        parent.Add(field);

        var binder = new ViewBinder();
        binder.AutoBind(parent, vm);

        vm.Name = "after";

        Assert.Equal("after", field.Text);
    }

    // -------------------------------------------------------------------
    // Button command — CanExecute drives Enabled
    // -------------------------------------------------------------------

    [Fact]
    public void AutoBind_Button_TracksCanExecute()
    {
        var vm = new AutoBindViewModel(); // CanSave == false initially
        var parent = new View();
        var button = new Button { Id = "SaveButton" };
        parent.Add(button);

        var binder = new ViewBinder();
        binder.AutoBind(parent, vm);

        Assert.False(button.Enabled); // initially disabled because CanSave == false

        vm.SetCanSave(true);

        Assert.True(button.Enabled);

        vm.SetCanSave(false);

        Assert.False(button.Enabled);
    }

    // -------------------------------------------------------------------
    // Skip paths — no throw, no unintended side-effects
    // -------------------------------------------------------------------

    [Fact]
    public void AutoBind_UnknownId_DoesNotThrow()
    {
        // "Unknown" maps to no VM member — should silently skip.
        var vm = new AutoBindViewModel();
        var parent = new View();
        var label = new Label { Id = "UnknownLabel" };
        parent.Add(label);

        var binder = new ViewBinder();
        var ex = Record.Exception(() => binder.AutoBind(parent, vm));

        Assert.Null(ex);
    }

    [Fact]
    public void AutoBind_WrongPropertyType_DoesNotThrow()
    {
        // "Count" is int, not string — BindStringByName returns early without throwing.
        var vm = new AutoBindViewModel();
        var parent = new View();
        var label = new Label { Id = "CountLabel" };
        parent.Add(label);

        var binder = new ViewBinder();
        var ex = Record.Exception(() => binder.AutoBind(parent, vm));

        Assert.Null(ex);
        Assert.Equal(string.Empty, label.Text); // left untouched
    }
}
