using SquidStd.Core.Extensions.Collections;

namespace SquidStd.Tests.Core.Extensions.Collections;

public class CollectionExtensionsTests
{
    [Fact]
    public void AddNotNull_AddsNonNull_SkipsNull()
    {
        var list = new List<string>();

        list.AddNotNull("value");
        list.AddNotNull(null);

        Assert.Equal(["value"], list);
    }

    [Fact]
    public void RandomElement_ReturnsAMemberOfTheCollection()
    {
        var items = new[] { 1, 2, 3, 4, 5 };

        for (var i = 0; i < 20; i++)
        {
            Assert.Contains(items.RandomElement(), items);
        }
    }

    [Fact]
    public void RandomElement_SingleItem_ReturnsIt()
        => Assert.Equal(7, new[] { 7 }.RandomElement());

    [Fact]
    public void RandomElement_EmptyCollection_Throws()
        => Assert.Throws<ArgumentException>(() => Array.Empty<int>().RandomElement());
}
