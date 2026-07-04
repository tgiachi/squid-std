using System.Runtime.CompilerServices;

namespace SquidStd.Core.Extensions.Strings;

/// <summary>
/// Ordinal case-insensitive comparison helpers for strings and character spans.
/// Instance-style overloads on nullable strings treat a null instance as "no match" (false / -1)
/// except <see cref="InsensitiveEquals(string?, string?)" /> and <see cref="InsensitiveCompare(string?, string?)" />.
/// </summary>
public static class InsensitiveStringHelpers
{
    /// <summary>Compares two spans ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InsensitiveCompare(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.CompareTo(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Compares two strings ignoring case (null sorts first).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InsensitiveCompare(this string? a, string? b)
        => string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the span contains <paramref name="b" /> ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveContains(this ReadOnlySpan<char> a, string? b)
        => b is not null && a.Contains(b.AsSpan(), StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the string contains <paramref name="b" /> ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveContains(this string? a, string? b)
        => a is not null && b is not null && a.Contains(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the span contains <paramref name="b" /> ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveContains(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Contains(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the string contains the character ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveContains(this string? a, char b)
        => a is not null && a.AsSpan().IndexOf([b], StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>Returns true when the string ends with <paramref name="b" /> ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveEndsWith(this string? a, string? b)
        => a is not null && b is not null && a.EndsWith(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the span ends with <paramref name="b" /> ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveEndsWith(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.EndsWith(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the spans are equal ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveEquals(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Equals(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the string equals the span ignoring case; a null string never matches.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveEquals(this string? a, ReadOnlySpan<char> b)
        => a is not null && a.AsSpan().Equals(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the strings are equal ignoring case; two nulls are equal.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveEquals(this string? a, string? b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Index of the character ignoring case, or -1 (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InsensitiveIndexOf(this string? a, char b)
        => a?.AsSpan().IndexOf([b], StringComparison.OrdinalIgnoreCase) ?? -1;

    /// <summary>Index of the substring ignoring case, or -1 (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InsensitiveIndexOf(this string? a, string? b)
        => a is null || b is null ? -1 : a.IndexOf(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Index of the substring starting at <paramref name="startIndex" /> ignoring case, or -1 (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InsensitiveIndexOf(this string? a, string? b, int startIndex)
        => a is null || b is null ? -1 : a.IndexOf(b, startIndex, StringComparison.OrdinalIgnoreCase);

    /// <summary>Index of <paramref name="b" /> in the span ignoring case, or -1.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InsensitiveIndexOf(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.IndexOf(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Copies the span into <paramref name="buffer" /> with every occurrence of <paramref name="b" /> removed, ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InsensitiveRemove(this ReadOnlySpan<char> a, ReadOnlySpan<char> b, Span<char> buffer, out int size)
        => a.Remove(b, StringComparison.OrdinalIgnoreCase, buffer, out size);

    /// <summary>Returns a new string equal to the span with every occurrence of <paramref name="b" /> removed, ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string InsensitiveRemove(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Remove(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns a new string with every occurrence of <paramref name="b" /> removed, ignoring case (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? InsensitiveRemove(this string? a, string b)
        => a?.Replace(b, string.Empty, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns the string with every occurrence of <paramref name="o" /> replaced by <paramref name="n" />, ignoring case (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? InsensitiveReplace(this string? a, string o, string? n)
        => a?.Replace(o, n, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the span starts with <paramref name="b" /> ignoring case.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveStartsWith(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.StartsWith(b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the string starts with <paramref name="b" /> ignoring case (null-safe).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsensitiveStartsWith(this string? a, string? b)
        => a is not null && b is not null && a.StartsWith(b, StringComparison.OrdinalIgnoreCase);
}
