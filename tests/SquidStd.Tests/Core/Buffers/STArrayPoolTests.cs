using SquidStd.Core.Buffers;
using SquidStd.Core.Types.Buffers;

namespace SquidStd.Tests.Core.Buffers;

public class STArrayPoolTests
{
    private readonly record struct RentItem(int Value);
    private readonly record struct ReuseItem(int Value);
    private readonly record struct StackItem(int Value);
    private readonly record struct ClearItem(int Value);
    private readonly record struct ForeignItem(int Value);
    private readonly record struct TrimHighItem(int Value);
    private readonly record struct TrimMediumItem(int Value);

    [Fact]
    public void Rent_ReturnsArrayAtLeastRequestedSize()
    {
        var array = STArrayPool<RentItem>.Shared.Rent(100);

        Assert.True(array.Length >= 100);
        Assert.Equal(128, array.Length); // bucket size 16 << index
    }

    [Fact]
    public void Rent_ZeroLength_ReturnsEmptySingleton()
    {
        var first = STArrayPool<RentItem>.Shared.Rent(0);
        var second = STArrayPool<RentItem>.Shared.Rent(0);

        Assert.Empty(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void Rent_NegativeLength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => STArrayPool<RentItem>.Shared.Rent(-1));
    }

    [Fact]
    public void ReturnThenRent_ReusesSameInstance()
    {
        var pool = STArrayPool<ReuseItem>.Shared;
        var array = pool.Rent(64);

        pool.Return(array);
        var again = pool.Rent(64);

        Assert.Same(array, again);
    }

    [Fact]
    public void Return_TwoArraysSameBucket_BothAreRecovered()
    {
        var pool = STArrayPool<StackItem>.Shared;
        var first = pool.Rent(64);
        var second = pool.Rent(64);

        pool.Return(first);
        pool.Return(second); // displaces first from the cache bucket into the stack

        var rentedA = pool.Rent(64);
        var rentedB = pool.Rent(64);

        Assert.Same(second, rentedA); // cache bucket hit first
        Assert.Same(first, rentedB);  // then the stack

        pool.Return(rentedA);
        pool.Return(rentedB);
    }

    [Fact]
    public void Return_WithClearArray_ZeroesContents()
    {
        var pool = STArrayPool<ClearItem>.Shared;
        var array = pool.Rent(16);
        array[0] = new(42);

        pool.Return(array, clearArray: true);
        var again = pool.Rent(16);

        Assert.Same(array, again);
        Assert.Equal(default, again[0]);
        pool.Return(again);
    }

    [Fact]
    public void Return_ForeignSizedArray_Throws()
    {
        // 100 is not a bucket size (16 << n), so it cannot come from this pool.
        Assert.Throws<ArgumentException>(() => STArrayPool<ForeignItem>.Shared.Return(new ForeignItem[100]));
    }

    [Fact]
    public void TrimCore_HighPressure_ReleasesCacheAndStacks()
    {
        var pool = STArrayPool<TrimHighItem>.Shared;
        var first = pool.Rent(64);
        var second = pool.Rent(64);
        pool.Return(first);
        pool.Return(second); // cache = second, stack = first

        pool.TrimCore(1_000, STArrayPoolMemoryPressureType.High);   // clears cache, stamps stack ticks
        pool.TrimCore(21_000, STArrayPoolMemoryPressureType.High);  // past the 10s stack threshold -> trims stack

        var rentedA = pool.Rent(64);
        var rentedB = pool.Rent(64);

        Assert.NotSame(second, rentedA);
        Assert.NotSame(first, rentedA);
        Assert.NotSame(first, rentedB);
    }

    [Fact]
    public void TrimCore_MediumPressure_ReleasesStaleCacheBuckets()
    {
        var pool = STArrayPool<TrimMediumItem>.Shared;
        var array = pool.Rent(64);
        pool.Return(array);

        pool.TrimCore(1_000, STArrayPoolMemoryPressureType.Medium);  // stamps cache bucket ticks
        pool.TrimCore(12_000, STArrayPoolMemoryPressureType.Medium); // past the 10s medium threshold -> releases

        var again = pool.Rent(64);

        Assert.NotSame(array, again);
    }

    [Theory]
    [InlineData(1, 16)]
    [InlineData(16, 16)]
    [InlineData(17, 32)]
    [InlineData(1024, 1024)]
    [InlineData(1025, 2048)]
    public void BucketMath_RoundsUpToNextBucketSize(int requested, int expected)
    {
        var index = STArrayPool<RentItem>.SelectBucketIndex(requested);

        Assert.Equal(expected, STArrayPool<RentItem>.GetMaxSizeForBucket(index));
    }
}
