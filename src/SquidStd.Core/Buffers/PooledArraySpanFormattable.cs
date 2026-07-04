using System.Buffers;

namespace SquidStd.Core.Buffers;

/// <summary>
/// Wraps an <see cref="ArrayPool{T}" />-rented char buffer as an <see cref="ISpanFormattable" /> so it can be
/// passed through interpolated string handlers without materializing an intermediate string.
/// <see cref="TryFormat" /> can only be called once: it returns the buffer to the pool. To read the
/// characters multiple times use <see cref="Chars" />; <see cref="ToString(string?, IFormatProvider?)" />
/// is idempotent (it caches the string and releases the buffer on first call).
/// </summary>
public struct PooledArraySpanFormattable : ISpanFormattable, IDisposable
{
    private char[]? _arrayToReturnToPool;
    private readonly int _length;
    private string? _value;

    /// <summary>
    /// Gets the written character slice while the buffer is still owned.
    /// </summary>
    public ReadOnlySpan<char> Chars => _arrayToReturnToPool.AsSpan(0, _length);

    /// <summary>
    /// Initializes the wrapper over a pooled buffer and the number of valid characters in it.
    /// </summary>
    /// <param name="arrayToReturnToPool">Buffer rented from <see cref="ArrayPool{T}" />.Shared.</param>
    /// <param name="length">Count of valid characters at the start of the buffer.</param>
    public PooledArraySpanFormattable(char[] arrayToReturnToPool, int length)
    {
        _arrayToReturnToPool = arrayToReturnToPool;
        _length = length;
        _value = null;
    }

    /// <summary>
    /// Converts the wrapper to a string. After the buffer has been released by ToString, the cached string is returned.
    /// </summary>
    public static implicit operator string(PooledArraySpanFormattable formattable)
        => formattable._value ?? new string(formattable.Chars);

    /// <inheritdoc />
    public override string ToString()
        => ToString(null, null);

    /// <summary>
    /// Materializes the content as a string. Idempotent: the first call caches the value and
    /// returns the buffer to the pool; later calls return the same instance.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (_value is null)
        {
            _value = new(Chars);
            ReleaseBuffer();
        }

        return _value;
    }

    /// <summary>
    /// Copies the content into <paramref name="destination" /> and returns the buffer to the pool.
    /// Single use: the wrapper must not be used again after a successful call.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (destination.Length < _length)
        {
            charsWritten = 0;

            return false;
        }

        Chars.CopyTo(destination);
        charsWritten = _length;
        ReleaseBuffer();

        return true;
    }

    private void ReleaseBuffer()
    {
        if (_arrayToReturnToPool is not null)
        {
            ArrayPool<char>.Shared.Return(_arrayToReturnToPool);
            _arrayToReturnToPool = null;
        }
    }

    /// <summary>
    /// Returns the buffer to the pool if it is still owned.
    /// </summary>
    public void Dispose()
        => ReleaseBuffer();
}
