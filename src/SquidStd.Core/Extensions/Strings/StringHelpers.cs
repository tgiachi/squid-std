using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SquidStd.Core.Buffers;

namespace SquidStd.Core.Extensions.Strings;

/// <summary>
/// General-purpose string manipulation helpers built on spans and the single-threaded array pool.
/// </summary>
public static class StringHelpers
{
    /// <summary>
    /// Appends <paramref name="text" /> to the builder, prefixing "a "/"an " when the builder is
    /// empty or a separating space otherwise.
    /// </summary>
    public static void AppendSpaceWithArticle(this ref ValueStringBuilder builder, string text, bool articleAn)
    {
        if (builder.Length != 0)
        {
            builder.Append(' ');
        }
        else
        {
            builder.Append(articleAn ? "an " : "a ");
        }

        builder.Append(text);
    }

    /// <summary>
    /// Uppercases the first letter of every space-separated word, leaving the word "the " untouched
    /// (title-style capitalization).
    /// </summary>
    public static string Capitalize(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return string.Create(
            value.Length,
            value,
            static (span, source) =>
            {
                source.CopyTo(span);
                var index = 0;

                while (index < span.Length)
                {
                    ReadOnlySpan<char> remaining = span[index..];

                    var isThe = remaining.StartsWith("the ", StringComparison.OrdinalIgnoreCase)
                                || remaining.Equals("the", StringComparison.OrdinalIgnoreCase);

                    if (!isThe)
                    {
                        span[index] = char.ToUpperInvariant(span[index]);
                    }

                    var nextSpace = remaining.IndexOf(' ');

                    if (nextSpace == -1)
                    {
                        break;
                    }

                    index += nextSpace + 1;
                }
            }
        );
    }

    /// <summary>
    /// Returns <paramref name="def" /> when the value is null, empty, or whitespace-only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DefaultIfNullOrEmpty(this string? value, string def)
        => string.IsNullOrWhiteSpace(value) ? def : value;

    /// <summary>
    /// Prefixes every line of a multiline string with <paramref name="indent" />.
    /// </summary>
    public static string IndentMultiline(this string str, string indent = "\t", string lineSeparator = "\n")
    {
        var parts = str.Split(lineSeparator);

        for (var i = 0; i < parts.Length; i++)
        {
            parts[i] = indent + parts[i];
        }

        return string.Join(lineSeparator, parts);
    }

    /// <summary>
    /// Finds the byte offset of the null terminator in a buffer of <paramref name="sizeT" />-byte
    /// elements (1 = bytes, 2 = UTF-16 chars, 4 = uints). Returns -1 when absent.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfTerminator(this Span<byte> buffer, int sizeT)
        => ((ReadOnlySpan<byte>)buffer).IndexOfTerminator(sizeT);

    /// <summary>
    /// Finds the byte offset of the null terminator in a buffer of <paramref name="sizeT" />-byte
    /// elements (1 = bytes, 2 = UTF-16 chars, 4 = uints). Returns -1 when absent.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfTerminator(this ReadOnlySpan<byte> buffer, int sizeT)
    {
        var index = sizeT switch
        {
            2 => MemoryMarshal.Cast<byte, char>(buffer).IndexOf((char)0),
            4 => MemoryMarshal.Cast<byte, uint>(buffer).IndexOf(0u),
            _ => buffer.IndexOf((byte)0)
        };

        return index == -1 ? -1 : index * sizeT;
    }

    /// <summary>
    /// Copies <paramref name="a" /> into <paramref name="buffer" /> with every occurrence of
    /// <paramref name="b" /> removed; <paramref name="size" /> receives the written length.
    /// Throws <see cref="ArgumentException" /> when the buffer is too small.
    /// </summary>
    public static void Remove(
        this ReadOnlySpan<char> a,
        ReadOnlySpan<char> b,
        StringComparison comparison,
        Span<char> buffer,
        out int size
    )
    {
        size = 0;

        if (a.IsEmpty)
        {
            return;
        }

        var sliced = a;

        while (true)
        {
            var indexOf = b.IsEmpty ? -1 : sliced.IndexOf(b, comparison);
            var chunk = indexOf == -1 ? sliced : sliced[..indexOf];

            if (size + chunk.Length > buffer.Length)
            {
                throw new ArgumentException("The output buffer is too small.", nameof(buffer));
            }

            chunk.CopyTo(buffer[size..]);
            size += chunk.Length;

            if (indexOf == -1)
            {
                break;
            }

            sliced = sliced[(indexOf + b.Length)..];
        }
    }

    /// <summary>
    /// Returns a new string equal to <paramref name="a" /> with every occurrence of
    /// <paramref name="b" /> removed. An empty input yields <see cref="string.Empty" />.
    /// </summary>
    public static string Remove(this ReadOnlySpan<char> a, ReadOnlySpan<char> b, StringComparison comparison)
    {
        if (a.IsEmpty)
        {
            return string.Empty;
        }

        var rented = STArrayPool<char>.Shared.Rent(a.Length);

        a.Remove(b, comparison, rented.AsSpan(0, a.Length), out var size);

        var result = new string(rented, 0, size);

        STArrayPool<char>.Shared.Return(rented);

        return result;
    }

    /// <summary>
    /// Replaces, in place, every character found in <paramref name="invalidChars" /> with the
    /// character at the same index in <paramref name="replacementChars" />.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="replacementChars" /> has a different length than <paramref name="invalidChars" />.</exception>
    public static void ReplaceAny(
        this Span<char> chars,
        ReadOnlySpan<char> invalidChars,
        ReadOnlySpan<char> replacementChars
    )
    {
        if (invalidChars.Length != replacementChars.Length)
        {
            throw new ArgumentException("Replacement characters must have the same length as invalid characters.", nameof(replacementChars));
        }

        while (true)
        {
            var indexOf = chars.IndexOfAny(invalidChars);

            if (indexOf == -1)
            {
                break;
            }

            chars[indexOf] = replacementChars[invalidChars.IndexOf(chars[indexOf])];
            chars = chars[(indexOf + 1)..];
        }
    }

    /// <summary>
    /// Copies the string into a buffer rented from <see cref="STArrayPool{T}" />. The CALLER owns
    /// the array and must return it via <c>STArrayPool&lt;char&gt;.Shared.Return(array)</c>; the
    /// buffer may be longer than the string.
    /// </summary>
    public static char[] ToPooledArray(this string str)
    {
        var chars = STArrayPool<char>.Shared.Rent(str.Length);

        str.CopyTo(chars);

        return chars;
    }

    /// <summary>
    /// Trims every line of a multiline string.
    /// </summary>
    public static string TrimMultiline(this string str, string lineSeparator = "\n")
    {
        var parts = str.Split(lineSeparator);

        for (var i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();
        }

        return string.Join(lineSeparator, parts);
    }

    /// <summary>
    /// Word-wraps the trimmed value into lines of at most <paramref name="perLine" /> characters,
    /// hard-breaking words longer than a line, up to <paramref name="maxLines" /> lines.
    /// Blank input yields an empty list.
    /// </summary>
    public static List<string> Wrap(this string? value, int perLine, int maxLines)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(perLine);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLines);

        var lines = new List<string>();
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            return lines;
        }

        var current = new System.Text.StringBuilder(perLine);

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var remaining = word.AsSpan();

            while (!remaining.IsEmpty)
            {
                var separatorLength = current.Length == 0 ? 0 : 1;
                var available = perLine - current.Length - separatorLength;

                if (remaining.Length <= available)
                {
                    current.Append(current.Length == 0 ? string.Empty : " ").Append(remaining);
                    remaining = default;
                }
                else if (current.Length == 0)
                {
                    // Hard-break a word longer than a whole line.
                    current.Append(remaining[..perLine]);
                    remaining = remaining[perLine..];
                }
                else
                {
                    lines.Add(current.ToString());
                    current.Clear();

                    if (lines.Count == maxLines)
                    {
                        return lines;
                    }
                }
            }
        }

        if (current.Length > 0 && lines.Count < maxLines)
        {
            lines.Add(current.ToString());
        }

        return lines;
    }
}
