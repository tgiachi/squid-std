// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using SquidStd.Core.Types.Buffers;

namespace SquidStd.Core.Buffers;

/// <summary>
/// Adaptation of <see cref="ArrayPool{T}" />.Shared (the runtime's TlsOverPerCoreLockedStacksArrayPool) for
/// single-threaded, unsynchronized usage.
/// </summary>
/// <remarks>
/// This pool is single-threaded by design: <see cref="Shared" /> is a per-<typeparamref name="T" />,
/// process-wide singleton that is NOT thread-safe and performs no locking or synchronization. Use it only
/// from code that already guarantees exclusive access (e.g. a single-threaded parser or formatter). For
/// concurrent access from multiple threads, use <see cref="ArrayPool{T}.Shared" /> instead. Returned
/// reference-type arrays are not cleared unless <c>clearArray</c> is passed as
/// <see langword="true" /> to <see cref="Return" />, so stale references may remain reachable through a
/// rented-then-returned array until it is rented again or overwritten.
/// </remarks>
public class STArrayPool<T> : ArrayPool<T>
{
#if DEBUG_ARRAYPOOL
    private static readonly ConditionalWeakTable<T[], STArrayPoolRentReturnStatus> _rentedArrays = new();
#endif

    private const int StackArraySize = 32;
    private const int BucketCount = 27; // SelectBucketIndex(1024 * 1024 * 1024 + 1)

    private static STArrayPoolBucket<T>[]? _cacheBuckets;
    private readonly STArrayPoolStack<T>?[] _buckets = new STArrayPoolStack<T>?[BucketCount];
    private int _trimCallbackCreated;

    /// <summary>
    /// Gets the process-wide, single-threaded shared instance for <typeparamref name="T" />. This instance
    /// is NOT thread-safe; see the class remarks.
    /// </summary>
    public static STArrayPool<T> Shared { get; } = new();

    private STArrayPool() { }

    /// <summary>
    /// Rents an array whose length is at least <paramref name="minimumLength" />. The array may contain
    /// previously used data and is not cleared unless it is later returned with <c>clearArray: true</c>.
    /// </summary>
    /// <param name="minimumLength">The minimum required length of the array.</param>
    /// <returns>An array whose length is at least <paramref name="minimumLength" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength" /> is negative.</exception>
    public override T[] Rent(int minimumLength)
    {
        T[]? buffer;

        var bucketIndex = SelectBucketIndex(minimumLength);
        var cachedBuckets = _cacheBuckets;

        if (cachedBuckets is not null && (uint)bucketIndex < (uint)cachedBuckets.Length)
        {
            buffer = cachedBuckets[bucketIndex].Array;

            if (buffer is not null)
            {
                cachedBuckets[bucketIndex].Array = null;
            #if DEBUG_ARRAYPOOL
                _rentedArrays.AddOrUpdate(
                    buffer,
                    new STArrayPoolRentReturnStatus { IsRented = true }
                );
            #endif
                return buffer;
            }
        }

        var buckets = _buckets;

        if ((uint)bucketIndex < (uint)buckets.Length)
        {
            var b = buckets[bucketIndex];

            if (b is not null)
            {
                buffer = b.TryPop();

                if (buffer is not null)
                {
                #if DEBUG_ARRAYPOOL
                    _rentedArrays.AddOrUpdate(
                        buffer,
                        new STArrayPoolRentReturnStatus { IsRented = true }
                    );
                #endif
                    return buffer;
                }
            }

            minimumLength = GetMaxSizeForBucket(bucketIndex);
        }

        if (minimumLength == 0)
        {
            // We aren't renting.
            return [];
        }

        if (minimumLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumLength));
        }

        var array = GC.AllocateUninitializedArray<T>(minimumLength);

    #if DEBUG_ARRAYPOOL
        _rentedArrays.AddOrUpdate(
            array,
            new STArrayPoolRentReturnStatus { IsRented = true, StackTrace = Environment.StackTrace }
        );
    #endif

        return array;
    }

    /// <summary>
    /// Returns an array previously rented from this pool. Reference-type contents are not cleared unless
    /// <paramref name="clearArray" /> is <see langword="true" />.
    /// </summary>
    /// <param name="array">The array to return. A <see langword="null" /> array is a no-op.</param>
    /// <param name="clearArray">Whether to zero the array's contents before caching it.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="array" />'s length does not match a bucket size produced by this pool.
    /// </exception>
    public override void Return(T[]? array, bool clearArray = false)
    {
        if (array is null)
        {
            return;
        }

        var bucketIndex = SelectBucketIndex(array.Length);
        var cacheBuckets = _cacheBuckets ?? InitializeBuckets();

        if ((uint)bucketIndex < (uint)cacheBuckets.Length)
        {
            if (clearArray)
            {
                Array.Clear(array);
            }

        #if DEBUG_ARRAYPOOL
            if (array.Length != GetMaxSizeForBucket(bucketIndex) || !_rentedArrays.TryGetValue(array, out var status))
            {
                throw new ArgumentException("Buffer is not from the pool", nameof(array));
            }

            if (!status!.IsRented)
            {
                throw new InvalidOperationException($"Array has already been returned.\nOriginal StackTrace:{status.StackTrace}\n");
            }

            // Mark it as returned
            status.IsRented = false;
            status.StackTrace = Environment.StackTrace;
        #else
            if (array.Length != GetMaxSizeForBucket(bucketIndex))
            {
                throw new ArgumentException("Buffer is not from the pool", nameof(array));
            }
        #endif

            ref var bucketArray = ref cacheBuckets[bucketIndex];
            var prev = bucketArray.Array;
            bucketArray = new(array);

            if (prev is not null)
            {
                var bucket = _buckets[bucketIndex] ?? CreateBucketStack(bucketIndex);
                bucket.TryPush(prev);
            }
        }
    }

    /// <summary>
    /// Trims pooled arrays according to current GC memory pressure. Invoked automatically on Gen2 collections.
    /// </summary>
    public bool Trim()
        => TrimCore(Environment.TickCount64, GetMemoryPressure());

    /// <summary>
    /// Core trimming logic driven by an explicit monotonic timestamp and memory pressure, so it can be
    /// exercised deterministically from tests without waiting on real GC or clock ticks.
    /// </summary>
    /// <param name="milliseconds">The current time, in milliseconds, from a monotonic clock such as <see cref="Environment.TickCount64" />.</param>
    /// <param name="pressure">The memory pressure to trim under.</param>
    /// <returns><see langword="true" />, always; matches the return contract used by the Gen2 GC callback.</returns>
    internal bool TrimCore(long milliseconds, STArrayPoolMemoryPressureType pressure)
    {
        var buckets = _buckets;

        for (var i = 0; i < buckets.Length; i++)
        {
            buckets[i]?.Trim(milliseconds, pressure, GetMaxSizeForBucket(i));
        }

        var cacheBuckets = _cacheBuckets;

        if (cacheBuckets is null)
        {
            return true;
        }

        // Under high pressure, release all cached buckets
        if (pressure == STArrayPoolMemoryPressureType.High)
        {
            Array.Clear(cacheBuckets);
        }
        else
        {
            uint threshold = pressure switch
            {
                STArrayPoolMemoryPressureType.Medium => 10000,
                _                                    => 30000
            };

            for (var i = 0; i < cacheBuckets.Length; i++)
            {
                ref var b = ref cacheBuckets[i];

                if (b.Array is null)
                {
                    continue;
                }

                var lastSeen = b.Ticks;

                if (lastSeen == 0)
                {
                    b.Ticks = milliseconds;
                }
                else if (milliseconds - lastSeen >= threshold)
                {
                    b.Array = null;
                }
            }
        }

        return true;
    }

    private STArrayPoolStack<T> CreateBucketStack(int bucketIndex)
        => _buckets[bucketIndex] = new(StackArraySize);

    private STArrayPoolBucket<T>[] InitializeBuckets()
    {
        Debug.Assert(_cacheBuckets is null, $"Non-null {nameof(_cacheBuckets)}");
        var buckets = new STArrayPoolBucket<T>[BucketCount];

        if (Interlocked.Exchange(ref _trimCallbackCreated, 1) == 0)
        {
            Gen2GcCallback.Register(o => ((STArrayPool<T>)o).Trim(), this);
        }

        return _cacheBuckets = buckets;
    }

    // Buffers are bucketed so that a request between 2^(n-1) + 1 and 2^n is given a buffer of 2^n
    // Bucket index is log2(bufferSize - 1) with the exception that buffers between 1 and 16 bytes
    // are combined, and the index is slid down by 3 to compensate.
    // Zero is a valid bufferSize, and it is assigned the highest bucket index so that zero-length
    // buffers are not retained by the pool. The pool will return the Array.Empty singleton for these.
    /// <summary>
    /// Computes the bucket index that would serve a rent request of <paramref name="bufferSize" />.
    /// </summary>
    /// <param name="bufferSize">The requested minimum buffer length.</param>
    /// <returns>The index of the smallest bucket able to satisfy the request.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int SelectBucketIndex(int bufferSize)
        => BitOperations.Log2(((uint)bufferSize - 1) | 15) - 3;

    /// <summary>
    /// Computes the maximum array length served by the given bucket index.
    /// </summary>
    /// <param name="binIndex">The bucket index, as produced by <see cref="SelectBucketIndex" />.</param>
    /// <returns>The maximum array length for that bucket.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetMaxSizeForBucket(int binIndex)
    {
        var maxSize = 16 << binIndex;
        Debug.Assert(maxSize >= 0);

        return maxSize;
    }

    /// <summary>
    /// Reads the current process memory load from the GC and classifies it into a trim pressure level.
    /// </summary>
    /// <returns>The current <see cref="STArrayPoolMemoryPressureType" />.</returns>
    internal static STArrayPoolMemoryPressureType GetMemoryPressure()
    {
        var memoryInfo = GC.GetGCMemoryInfo();

        if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * 0.90)
        {
            return STArrayPoolMemoryPressureType.High;
        }

        if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * 0.70)
        {
            return STArrayPoolMemoryPressureType.Medium;
        }

        return STArrayPoolMemoryPressureType.Low;
    }
}
