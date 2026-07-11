using SquidStd.Core.Rng;
using SquidStd.Core.Types;

namespace SquidStd.Tests.Rng;

public class RandomFactoryTests
{
    public static IEnumerable<object[]> Algorithms =>
        Enum.GetValues<RandomAlgorithmType>().Select(a => new object[] { a });

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Create_WithAlgorithm_ProducesValues(RandomAlgorithmType algorithm)
    {
        var random = RandomFactory.Create(algorithm, 123u);

        var value = random.NextInt(0, 100);

        Assert.InRange(value, 0, 99);
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Create_SameSeed_IsReproducible(RandomAlgorithmType algorithm)
    {
        var a = RandomFactory.Create(algorithm, 999u);
        var b = RandomFactory.Create(algorithm, 999u);

        var first = Enumerable.Range(0, 50).Select(_ => a.NextInt()).ToArray();
        var second = Enumerable.Range(0, 50).Select(_ => b.NextInt()).ToArray();

        Assert.Equal(first, second);
    }

    [Fact]
    public void Create_DifferentSeeds_ProduceDifferentSequences()
    {
        var a = RandomFactory.Create(1u);
        var b = RandomFactory.Create(2u);

        var first = Enumerable.Range(0, 20).Select(_ => a.NextInt()).ToArray();
        var second = Enumerable.Range(0, 20).Select(_ => b.NextInt()).ToArray();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void NextInt_WithBounds_StaysInRange()
    {
        var random = RandomFactory.Create(7u);

        for (var i = 0; i < 10_000; i++)
        {
            Assert.InRange(random.NextInt(10, 20), 10, 19);
        }
    }

    [Fact]
    public void NextDouble_IsInUnitInterval()
    {
        var random = RandomFactory.Create(7u);

        for (var i = 0; i < 10_000; i++)
        {
            var value = random.NextDouble();
            Assert.InRange(value, 0.0, 1.0);
            Assert.NotEqual(1.0, value);
        }
    }

    [Theory]
    [InlineData(0.0, false)]
    [InlineData(1.0, true)]
    public void NextBool_AtExtremes_IsDeterministic(double probability, bool expected)
    {
        var random = RandomFactory.Create(7u);

        for (var i = 0; i < 1_000; i++)
        {
            Assert.Equal(expected, random.NextBool(probability));
        }
    }

    [Fact]
    public void NextGaussian_ApproximatesRequestedMean()
    {
        var random = RandomFactory.Create(7u);
        var sum = 0.0;
        const int samples = 200_000;

        for (var i = 0; i < samples; i++)
        {
            sum += random.NextGaussian(5.0, 2.0);
        }

        Assert.Equal(5.0, sum / samples, 1);
    }

    [Fact]
    public void Shuffle_PreservesMultiset()
    {
        var random = RandomFactory.Create(7u);
        var items = Enumerable.Range(0, 100).ToList();

        random.Shuffle(items);

        Assert.Equal(Enumerable.Range(0, 100), items.OrderBy(x => x));
    }

    [Fact]
    public void Pick_ReturnsMemberOfList()
    {
        var random = RandomFactory.Create(7u);
        var items = new[] { "a", "b", "c" };

        for (var i = 0; i < 100; i++)
        {
            Assert.Contains(random.Pick(items), items);
        }
    }

    [Fact]
    public void Pick_EmptyList_Throws()
    {
        var random = RandomFactory.Create(7u);

        Assert.Throws<ArgumentException>(() => random.Pick(Array.Empty<int>()));
    }

    [Fact]
    public void Shared_ProducesValues()
    {
        var value = RandomFactory.Shared.NextInt(0, 10);

        Assert.InRange(value, 0, 9);
    }
}
