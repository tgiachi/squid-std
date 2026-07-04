using System.Runtime.CompilerServices;

namespace SquidStd.Core.Extensions.Strings;

/// <summary>
/// Ordinal (culture-independent, case-sensitive) comparison helpers for strings and character spans.
/// Instance-style overloads on nullable strings treat a null instance as "no match" (false / -1)
/// except <see cref="EqualsOrdinal(string?, string?)" /> and <see cref="CompareOrdinal(string?, string?)" />,
/// which follow <see cref="string.Equals(string?, string?, StringComparison)" /> semantics.
/// </summary>
public static class OrdinalStringHelpers
{
    /// <summary>Compares two spans with ordinal semantics.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.CompareTo(b, StringComparison.Ordinal);

    /// <summary>Compares two strings with ordinal semantics (null sorts first).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareOrdinal(this string? a, string? b)
        => string.CompareOrdinal(a, b);

    /// <summary>Returns true when the span contains <paramref name="b" /> (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this ReadOnlySpan<char> a, string? b)
        => b is not null && a.Contains(b.AsSpan(), StringComparison.Ordinal);

    /// <summary>Returns true when the string contains <paramref name="b" /> (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this string? a, string? b)
        => a is not null && b is not null && a.Contains(b, StringComparison.Ordinal);

    /// <summary>Returns true when the span contains <paramref name="b" /> (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Contains(b, StringComparison.Ordinal);

    /// <summary>Returns true when the string contains the character.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this string? a, char b)
        => a is not null && a.Contains(b);

    /// <summary>Returns true when the string ends with <paramref name="b" /> (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithOrdinal(this string? a, string? b)
        => a is not null && b is not null && a.EndsWith(b, StringComparison.Ordinal);

    /// <summary>Returns true when the span ends with the character.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithOrdinal(this ReadOnlySpan<char> a, char b)
        => a.Length > 0 && a[^1] == b;

    /// <summary>Returns true when the span ends with <paramref name="b" /> (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.EndsWith(b, StringComparison.Ordinal);

    /// <summary>Returns true when the span equals <paramref name="b" /> (ordinal); a null string never matches.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdinal(this ReadOnlySpan<char> a, string? b)
        => b is not null && a.SequenceEqual(b.AsSpan());

    /// <summary>Returns true when the strings are equal (ordinal); two nulls are equal.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdinal(this string? a, string? b)
        => string.Equals(a, b, StringComparison.Ordinal);

    /// <summary>Index of the character, or -1 (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this string? a, char b)
        => a?.IndexOf(b) ?? -1;

    /// <summary>Index of the substring (ordinal), or -1 (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this string? a, string? b)
        => a is null || b is null ? -1 : a.IndexOf(b, StringComparison.Ordinal);

    /// <summary>Index of the substring starting at <paramref name="startIndex" /> (ordinal), or -1 (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this string? a, string? b, int startIndex)
        => a is null || b is null ? -1 : a.IndexOf(b, startIndex, StringComparison.Ordinal);

    /// <summary>Index of the character in the span, or -1.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this ReadOnlySpan<char> a, char b)
        => a.IndexOf(b);

    /// <summary>Index of <paramref name="b" /> in the span (ordinal), or -1.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.IndexOf(b, StringComparison.Ordinal);

    /// <summary>Returns the string with every occurrence of <paramref name="b" /> removed (ordinal, null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? RemoveOrdinal(this string? a, string b)
        => a?.Replace(b, string.Empty, StringComparison.Ordinal);

    /// <summary>Copies the span into <paramref name="buffer" /> with every occurrence of <paramref name="b" /> removed (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b, Span<char> buffer, out int size)
        => a.Remove(b, StringComparison.Ordinal, buffer, out size);

    /// <summary>Returns a new string equal to the span with every occurrence of <paramref name="b" /> removed (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RemoveOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Remove(b, StringComparison.Ordinal);

    /// <summary>Returns the string with every occurrence of <paramref name="o" /> replaced by <paramref name="n" /> (ordinal, null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ReplaceOrdinal(this string? a, string o, string? n)
        => a?.Replace(o, n, StringComparison.Ordinal);

    /// <summary>Returns true when the span starts with the character.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this ReadOnlySpan<char> a, char b)
        => a.Length > 0 && a[0] == b;

    /// <summary>Returns true when the span starts with <paramref name="b" /> (ordinal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.StartsWith(b, StringComparison.Ordinal);

    /// <summary>Returns true when the string starts with <paramref name="b" /> (ordinal, null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this string? a, string? b)
        => a is not null && b is not null && a.StartsWith(b, StringComparison.Ordinal);
}
