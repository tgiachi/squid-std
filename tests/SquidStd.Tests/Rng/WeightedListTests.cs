using SquidStd.Core.Rng;

namespace SquidStd.Tests.Rng;

public class WeightedListTests
{
    [Fact]
    public void Add_TracksCountAndTotalWeight()
    {
        var list = new WeightedList<string>();

        list.Add("a", 2.0);
        list.Add("b", 3.0);

        Assert.Equal(2, list.Count);
        Assert.Equal(5.0, list.TotalWeight, 6);
    }

    [Fact]
    public void Next_SingleItem_AlwaysReturnsIt()
    {
        var random = RandomFactory.Create(7u);
        var list = new WeightedList<string>();
        list.Add("only", 1.0);

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal("only", list.Next(random));
        }
    }

    [Fact]
    public void Next_RespectsWeights()
    {
        var random = RandomFactory.Create(7u);
        var list = new WeightedList<string>();
        list.Add("rare", 1.0);
        list.Add("common", 99.0);

        var common = 0;
        const int draws = 100_000;

        for (var i = 0; i < draws; i++)
        {
            if (list.Next(random) == "common")
            {
                common++;
            }
        }

        var ratio = (double)common / draws;
        Assert.InRange(ratio, 0.97, 1.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Add_NonPositiveOrNonFiniteWeight_Throws(double weight)
    {
        var list = new WeightedList<string>();

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Add("x", weight));
    }

    [Fact]
    public void Next_EmptyList_Throws()
    {
        var random = RandomFactory.Create(7u);
        var list = new WeightedList<string>();

        Assert.Throws<InvalidOperationException>(() => list.Next(random));
    }
}
