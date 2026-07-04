using System.Runtime.CompilerServices;
using SquidStd.Core.Types.Buffers;

namespace SquidStd.Core.Buffers;

/// <summary>
/// A small fixed-capacity LIFO stack of arrays, used by <see cref="STArrayPool{T}" /> to hold onto arrays
/// that overflow the single-slot cache bucket for a given size.
/// </summary>
internal sealed class STArrayPoolStack<T>
{
    private readonly T[]?[] _arrays;
    private int _count;
    private long _ticks;

    /// <summary>
    /// Initializes a new instance of the <see cref="STArrayPoolStack{T}" /> class with the given capacity.
    /// </summary>
    /// <param name="stackArraySize">The maximum number of arrays the stack can hold.</param>
    public STArrayPoolStack(int stackArraySize)
    {
        _arrays = new T[]?[stackArraySize];
    }

    /// <summary>
    /// Trims idle arrays from the stack according to the current memory pressure.
    /// </summary>
    /// <param name="now">The current monotonic tick count, in milliseconds.</param>
    /// <param name="pressure">The current GC memory pressure.</param>
    /// <param name="bucketSize">The maximum array size for this stack's bucket.</param>
    public void Trim(long now, STArrayPoolMemoryPressureType pressure, int bucketSize)
    {
        if (_count == 0)
        {
            return;
        }

        var threshold = pressure == STArrayPoolMemoryPressureType.High ? 10000 : 60000;

        if (_ticks == 0)
        {
            _ticks = now;

            return;
        }

        if (now - _ticks <= threshold)
        {
            return;
        }

        var trimCount = 1;

        switch (pressure)
        {
            case STArrayPoolMemoryPressureType.Medium:
                trimCount = 2;

                break;
            case STArrayPoolMemoryPressureType.High:
                if (bucketSize > 16384)
                {
                    trimCount++;
                }

                var size = Unsafe.SizeOf<T>();

                if (size > 32)
                {
                    trimCount += 2;
                }
                else if (size > 16)
                {
                    trimCount++;
                }

                break;
        }

        while (_count > 0 && trimCount-- > 0)
        {
            _arrays[--_count] = null;
        }
    }

    /// <summary>
    /// Attempts to pop an array from the top of the stack.
    /// </summary>
    /// <returns>The popped array, or <see langword="null" /> if the stack is empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[]? TryPop()
    {
        var arrays = _arrays;
        var count = _count - 1;

        if ((uint)count < (uint)arrays.Length)
        {
            var arr = arrays[count];
            arrays[count] = null;
            _count = count;

            return arr;
        }

        return null;
    }

    /// <summary>
    /// Attempts to push an array onto the stack.
    /// </summary>
    /// <param name="array">The array to push.</param>
    /// <returns><see langword="true" /> if the array was pushed; <see langword="false" /> if the stack is full.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPush(T[] array)
    {
        var arrays = _arrays;
        var count = _count;

        if ((uint)count < (uint)arrays.Length)
        {
            arrays[count] = array;
            _count = count + 1;

            return true;
        }

        return false;
    }
}
