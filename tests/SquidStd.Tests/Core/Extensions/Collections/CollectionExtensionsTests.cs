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
}
