// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SquidStd.Core.Buffers;

/// <summary>
/// A single-use, stack-friendly string builder built on a mutable <see cref="Span{T}" /> buffer instead of
/// <see cref="System.Text.StringBuilder" />'s linked chunks.
/// </summary>
/// <remarks>
/// This is a <see langword="ref struct" />: it must stay on the stack and cannot be boxed, stored in a
/// field of a non-ref struct, or captured by a lambda/async method. With <c>mt: false</c> (the default),
/// growth buffers are rented from <see cref="STArrayPool{T}" />.Shared, which is NOT thread-safe, so the
/// builder must be created, appended to, and disposed from a single thread. Pass <c>mt: true</c> to switch
/// to <see cref="ArrayPool{T}" />.Shared when the builder (or a buffer it grew into) might cross threads.
/// </remarks>
public ref struct ValueStringBuilder
{
    private char[]? _arrayToReturnToPool;
    private Span<char> _chars;
    private readonly bool _mt;

    private ArrayPool<char> ArrayPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _mt ? ArrayPool<char>.Shared : STArrayPool<char>.Shared;
    }

    /// <summary>
    /// Initializes a builder whose buffer starts as a copy of <paramref name="initialString" />.
    /// </summary>
    /// <param name="initialString">The characters to seed the builder with.</param>
    /// <param name="mt">
    /// When <see langword="true" />, buffers grow via <see cref="ArrayPool{T}" />.Shared instead of the
    /// single-threaded <see cref="STArrayPool{T}" />.Shared.
    /// </param>
    /// <remarks>If this ctor is used, you cannot pass in stackalloc ROS for append/replace.</remarks>
    public ValueStringBuilder(ReadOnlySpan<char> initialString, bool mt = false) : this(initialString.Length, mt)
    {
        Append(initialString);
    }

    /// <summary>
    /// Initializes a builder over <paramref name="initialBuffer" /> seeded with a copy of
    /// <paramref name="initialString" />.
    /// </summary>
    /// <param name="initialString">The characters to seed the builder with.</param>
    /// <param name="initialBuffer">The initial backing storage, e.g. a stackalloc'd span.</param>
    /// <param name="mt">
    /// When <see langword="true" />, buffers grow via <see cref="ArrayPool{T}" />.Shared instead of the
    /// single-threaded <see cref="STArrayPool{T}" />.Shared.
    /// </param>
    public ValueStringBuilder(ReadOnlySpan<char> initialString, Span<char> initialBuffer, bool mt = false) : this(
        initialBuffer,
        mt
    )
    {
        Append(initialString);
    }

    /// <summary>
    /// Initializes an empty builder over <paramref name="initialBuffer" />, e.g. a stackalloc'd span.
    /// </summary>
    /// <param name="initialBuffer">The initial backing storage.</param>
    /// <param name="mt">
    /// When <see langword="true" />, buffers grow via <see cref="ArrayPool{T}" />.Shared instead of the
    /// single-threaded <see cref="STArrayPool{T}" />.Shared.
    /// </param>
    public ValueStringBuilder(Span<char> initialBuffer, bool mt = false)
    {
        _mt = mt;
        _arrayToReturnToPool = null;
        _chars = initialBuffer;
        Length = 0;
    }

    /// <summary>
    /// Initializes an empty builder with a buffer rented at the given capacity.
    /// </summary>
    /// <param name="initialCapacity">The minimum initial buffer capacity.</param>
    /// <param name="mt">
    /// When <see langword="true" />, the initial buffer and any growth are rented from
    /// <see cref="ArrayPool{T}" />.Shared instead of the single-threaded <see cref="STArrayPool{T}" />.Shared.
    /// </param>
    /// <remarks>If this ctor is used, you cannot pass in stackalloc ROS for append/replace.</remarks>
    public ValueStringBuilder(int initialCapacity, bool mt = false)
    {
        _mt = mt;
        Length = 0;
        _arrayToReturnToPool = (_mt ? ArrayPool<char>.Shared : STArrayPool<char>.Shared).Rent(initialCapacity);
        _chars = _arrayToReturnToPool;
    }

    /// <summary>Gets the number of characters currently written to the builder.</summary>
    public int Length { get; private set; }

    /// <summary>Gets the current capacity of the underlying buffer.</summary>
    public int Capacity => _chars.Length;

    /// <summary>Gets a mutable reference to the character at the given index.</summary>
    /// <param name="index">The zero-based character index.</param>
    public ref char this[int index] => ref _chars[index];

    /// <summary>Returns the underlying storage of the builder.</summary>
    public Span<char> RawChars => _chars;

    /// <summary>
    /// Appends <paramref name="value" /> using <see cref="ISpanFormattable" /> or <see cref="IFormattable" />
    /// when available, falling back to <see cref="object.ToString" />.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <param name="format">An optional format string passed to the value's formatter.</param>
    public void Append<T>(T value, string? format = null)
    {
        if (value is IFormattable)
        {
            if (value is ISpanFormattable)
            {
                var destination = _chars[Length..];
                int charsWritten;

                while (!((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, default))
                {
                    Grow(1);
                    destination = _chars[Length..];
                }

                if ((uint)charsWritten > (uint)destination.Length)
                {
                    throw new FormatException("Invalid string");
                }

                Length += charsWritten;
            }
            else
            {
                Append(((IFormattable)value).ToString(format, default)); // constrained call avoiding boxing for value types
            }
        }
        else if (value is not null)
        {
            Append(value.ToString());
        }
    }

    /// <summary>Appends the text produced by a <see cref="RawInterpolatedStringHandler" />.</summary>
    /// <param name="handler">The interpolated string handler holding the formatted text.</param>
    // Compiler generated
    public void Append(RawInterpolatedStringHandler handler)
    {
        Append(handler.Text);
        handler.Clear();
    }

    /// <summary>
    /// Appends the text produced by a <see cref="RawInterpolatedStringHandler" /> using the given
    /// <paramref name="formatProvider" />.
    /// </summary>
    /// <param name="formatProvider">The format provider passed to the interpolated string handler.</param>
    /// <param name="handler">The interpolated string handler holding the formatted text.</param>
    // Compiler generated
    public void Append(
        IFormatProvider? formatProvider,
        [InterpolatedStringHandlerArgument("formatProvider")]
        RawInterpolatedStringHandler handler
    )
    {
        Append(handler.Text);
        handler.Clear();
    }

    /// <summary>Appends a string. A no-op if <paramref name="s" /> is <see langword="null" />.</summary>
    /// <param name="s">The string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
        if (s == null)
        {
            return;
        }

        var pos = Length;

        if (s.Length == 1 &&
            (uint)pos <
            (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        {
            _chars[pos] = s[0];
            Length = pos + 1;
        }
        else
        {
            AppendSlow(s);
        }
    }

    /// <summary>Appends the same character <paramref name="count" /> times.</summary>
    /// <param name="c">The character to append.</param>
    /// <param name="count">The number of times to append it.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, int count)
    {
        if (Length > _chars.Length - count)
        {
            Grow(count);
        }

        var dst = _chars.Slice(Length, count);

        for (var i = 0; i < dst.Length; i++)
        {
            dst[i] = c;
        }
        Length += count;
    }

    /// <summary>Appends the characters of <paramref name="value" />.</summary>
    /// <param name="value">The characters to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> value)
    {
        var pos = Length;

        if (pos > _chars.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_chars[Length..]);
        Length += value.Length;
    }

    /// <summary>
    /// Appends <paramref name="s" /> followed by <see cref="Environment.NewLine" />. A no-op if
    /// <paramref name="s" /> is <see langword="null" />: nothing is appended, not even the newline.
    /// </summary>
    /// <param name="s">The string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(string? s)
    {
        if (s == null)
        {
            return;
        }

        // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        if (s.Length == 1)
        {
            Append(s[0]);
        }
        else
        {
            AppendSlow(s);
        }

        Append(Environment.NewLine);
    }

    /// <summary>
    /// Reserves <paramref name="length" /> characters at the end of the builder and returns a span over them
    /// for the caller to fill in directly.
    /// </summary>
    /// <param name="length">The number of characters to reserve.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        var origPos = Length;

        if (origPos > _chars.Length - length)
        {
            Grow(length);
        }

        Length = origPos + length;

        return _chars.Slice(origPos, length);
    }

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length" /></param>
    public ReadOnlySpan<char> AsSpan(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            _chars[Length] = '\0';
        }

        return _chars[..Length];
    }

    /// <summary>Returns a span over the written contents of the builder.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan()
        => _chars[..Length];

    /// <summary>Returns a span over the written contents of the builder starting at <paramref name="start" />.</summary>
    /// <param name="start">The zero-based index to start the span at.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(int start)
        => _chars[start..];

    /// <summary>Returns a span over a sub-range of the written contents of the builder.</summary>
    /// <param name="start">The zero-based index to start the span at.</param>
    /// <param name="length">The number of characters to include.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(int start, int length)
        => _chars.Slice(start, length);

    /// <summary>Creates a builder with a rented buffer of at least <paramref name="capacity" /> characters.</summary>
    /// <param name="capacity">The minimum initial buffer capacity.</param>
    /// <param name="mt">
    /// When <see langword="true" />, the buffer is rented from <see cref="ArrayPool{T}" />.Shared instead of
    /// the single-threaded <see cref="STArrayPool{T}" />.Shared.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueStringBuilder Create(int capacity = 64, bool mt = false)
        => new(capacity, mt);

    /// <summary>Creates a builder whose buffers are rented from <see cref="ArrayPool{T}" />.Shared.</summary>
    /// <param name="capacity">The minimum initial buffer capacity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueStringBuilder CreateMT(int capacity = 64)
        => new(capacity, true);

    /// <summary>Ensures the buffer can hold at least <paramref name="capacity" /> characters, growing it if needed.</summary>
    /// <param name="capacity">The minimum required capacity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int capacity)
    {
        if (capacity > _chars.Length)
        {
            Grow(capacity - Length);
        }
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length" />
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref char GetPinnableReference()
        => ref MemoryMarshal.GetReference(_chars);

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length" /></param>
    public ref char GetPinnableReference(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            _chars[Length] = '\0';
        }

        return ref MemoryMarshal.GetReference(_chars);
    }

    /// <summary>Inserts the same character <paramref name="count" /> times at <paramref name="index" />, shifting existing content right.</summary>
    /// <param name="index">The zero-based index to insert at.</param>
    /// <param name="value">The character to insert.</param>
    /// <param name="count">The number of times to insert it.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, char value, int count)
    {
        if (Length > _chars.Length - count)
        {
            Grow(count);
        }

        var remaining = Length - index;
        _chars.Slice(index, remaining).CopyTo(_chars[(index + count)..]);
        _chars.Slice(index, count).Fill(value);
        Length += count;
    }

    /// <summary>
    /// Inserts <paramref name="s" /> at <paramref name="index" />, shifting existing content right. A no-op if
    /// <paramref name="s" /> is <see langword="null" />.
    /// </summary>
    /// <param name="index">The zero-based index to insert at.</param>
    /// <param name="s">The string to insert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, string? s)
    {
        if (s == null)
        {
            return;
        }

        var count = s.Length;

        if (Length > _chars.Length - count)
        {
            Grow(count);
        }

        var remaining = Length - index;
        _chars.Slice(index, remaining).CopyTo(_chars[(index + count)..]);
        s.AsSpan().CopyTo(_chars[index..]);
        Length += count;
    }

    /// <summary>Removes <paramref name="length" /> characters starting at <paramref name="startIndex" />.</summary>
    /// <param name="startIndex">The zero-based index to start removing from.</param>
    /// <param name="length">The number of characters to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="length" /> or <paramref name="startIndex" /> is negative, or the range extends past
    /// <see cref="Length" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int startIndex, int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (length > Length - startIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (startIndex == 0)
        {
            _chars = _chars[length..];
        }
        else if (startIndex + length == Length)
        {
            _chars = _chars[..startIndex];
        }
        else
        {
            // Somewhere in the middle, this will be slow
            _chars[(startIndex + length)..].CopyTo(_chars[startIndex..]);
        }

        Length -= length;
    }

    /// <summary>Replaces all occurrences of <paramref name="oldChar" /> with <paramref name="newChar" /> within a range.</summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The replacement character.</param>
    /// <param name="startIndex">The zero-based index the range starts at.</param>
    /// <param name="count">The number of characters the range covers.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex" /> or <paramref name="count" /> is out of range for the current
    /// <see cref="Length" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Replace(char oldChar, char newChar, int startIndex, int count)
    {
        var currentLength = Length;

        if ((uint)startIndex > (uint)currentLength)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (count < 0 || startIndex > currentLength - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var slice = _chars.Slice(startIndex, count);

        while (true)
        {
            var indexOf = slice.IndexOf(oldChar);

            if (indexOf == -1)
            {
                break;
            }

            slice[indexOf] = newChar;
            slice = slice[(indexOf + 1)..];
        }
    }

    /// <summary>Replaces occurrences of characters in <paramref name="oldChars" /> with the character at the same position in <paramref name="newChars" />, within a range.</summary>
    /// <param name="oldChars">The characters to look for.</param>
    /// <param name="newChars">The replacement characters, aligned by index with <paramref name="oldChars" />.</param>
    /// <param name="startIndex">The zero-based index the range starts at.</param>
    /// <param name="count">The number of characters the range covers.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex" /> or <paramref name="count" /> is out of range for the current
    /// <see cref="Length" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReplaceAny(ReadOnlySpan<char> oldChars, ReadOnlySpan<char> newChars, int startIndex, int count)
    {
        var currentLength = Length;

        if ((uint)startIndex > (uint)currentLength)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (count < 0 || startIndex > currentLength - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var slice = _chars.Slice(startIndex, count);

        while (true)
        {
            var indexOf = slice.IndexOfAny(oldChars);

            if (indexOf == -1)
            {
                break;
            }

            var chr = slice[indexOf];

            slice[indexOf] = newChars[oldChars.IndexOf(chr)];
            slice = slice[(indexOf + 1)..];
        }
    }

    /// <summary>Resets <see cref="Length" /> to zero without releasing the buffer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
        => Length = 0;

    /// <summary>Returns the written contents as a new <see cref="string" />.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => _chars[..Length].ToString();

    /// <summary>Attempts to copy the written contents into <paramref name="destination" />.</summary>
    /// <param name="destination">The span to copy into.</param>
    /// <param name="charsWritten">The number of characters written, or zero if the copy failed.</param>
    /// <returns><see langword="true" /> if the contents fit in <paramref name="destination" />; otherwise <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (_chars[..Length].TryCopyTo(destination))
        {
            charsWritten = Length;

            return true;
        }

        charsWritten = 0;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendSlow(string s)
    {
        var pos = Length;

        if (pos > _chars.Length - s.Length)
        {
            Grow(s.Length);
        }

        s.AsSpan().CopyTo(_chars[pos..]);
        Length += s.Length;
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos" /> to
    /// <see cref="Length" /> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        var poolArray = ArrayPool.Rent(Math.Max(Length + additionalCapacityBeyondPos, _chars.Length * 2));

        _chars[..Length].CopyTo(poolArray);

        var toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = poolArray;

        if (toReturn != null)
        {
            ArrayPool.Return(toReturn);
        }
    }

    /// <summary>Releases the rented buffer, if any, back to its pool.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_arrayToReturnToPool != null)
        {
            ArrayPool.Return(_arrayToReturnToPool);
        }

        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
    }
}
