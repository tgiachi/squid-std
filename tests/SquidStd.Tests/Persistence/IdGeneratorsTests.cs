using SquidStd.Persistence.Data;

namespace SquidStd.Tests.Persistence;

public class IdGeneratorsTests
{
    [Fact]
    public void Int32_InitialIsSeed_NextIncrements()
    {
        var generator = IdGenerators.Int32(seed: 1);

        Assert.Equal(1, generator.Initial);
        Assert.Equal(6, generator.Next(5));
    }

    [Fact]
    public void Int64_InitialIsSeed_NextIncrements()
    {
        var generator = IdGenerators.Int64(seed: 100);

        Assert.Equal(100L, generator.Initial);
        Assert.Equal(101L, generator.Next(100));
    }
}
