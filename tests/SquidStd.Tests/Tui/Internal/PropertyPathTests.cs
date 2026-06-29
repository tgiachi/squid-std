using SquidStd.Tui.Internal;

namespace SquidStd.Tests.Tui.Internal;

public class PropertyPathTests
{
    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;
        public int Count;
    }

    [Fact]
    public void NameOf_Property_ReturnsName()
    {
        Assert.Equal("Name", PropertyPath.NameOf<Sample, string>(s => s.Name));
    }

    [Fact]
    public void NameOf_Field_ReturnsName()
    {
        Assert.Equal("Count", PropertyPath.NameOf<Sample, int>(s => s.Count));
    }

    [Fact]
    public void Setter_Property_WritesValue()
    {
        var setter = PropertyPath.Setter<Sample, string>(s => s.Name);
        var target = new Sample();

        setter(target, "abc");

        Assert.Equal("abc", target.Name);
    }

    [Fact]
    public void NameOf_NonMemberExpression_Throws()
    {
        Assert.Throws<ArgumentException>(() => PropertyPath.NameOf<Sample, int>(s => s.Count + 1));
    }
}
