using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SquidStd.Tui.Dsl;
using SquidStd.Tui.Internal;
using SquidStd.Tui.Types.Tui;

namespace SquidStd.Tests.Tui.Dsl;

public class UiFactoryTests
{
    private sealed class SampleViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICommand Save { get; } = new RelayCommand(() => { });
    }

    private readonly UiFactory<SampleViewModel> _ui = new();

    [Fact]
    public void Label_ProducesLabelNode()
    {
        var node = _ui.Label(x => x.Title);

        Assert.Equal(nameof(SampleViewModel.Title), PropertyPath.NameOf(node.Text));
    }

    [Fact]
    public void TextField_DefaultsToTwoWay_AndOneWayIsExplicit()
    {
        Assert.Equal(BindMode.TwoWay, _ui.TextField(x => x.Name).Mode);
        Assert.Equal(BindMode.OneWay, _ui.TextField(x => x.Name, BindMode.OneWay).Mode);
    }

    [Fact]
    public void Button_ProducesButtonNode()
    {
        var node = _ui.Button("Save", x => x.Save);

        Assert.Equal("Save", node.Caption);
    }

    [Fact]
    public void VStack_And_HStack_SetOrientationAndChildren()
    {
        var v = _ui.VStack(_ui.Label(x => x.Title), _ui.Label(x => x.Name));
        var h = _ui.HStack(_ui.Label(x => x.Title));

        Assert.Equal(StackOrientation.Vertical, v.Orientation);
        Assert.Equal(2, v.Children.Count);
        Assert.Equal(StackOrientation.Horizontal, h.Orientation);
        Assert.Single(h.Children);
    }
}
