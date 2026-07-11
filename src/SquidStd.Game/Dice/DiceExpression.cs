using System.Globalization;
using SquidStd.Core.Interfaces.Rng;
using SquidStd.Core.Utils;

namespace SquidStd.Game.Dice;

/// <summary>
/// A parsed dice-notation expression of the form <c>[N]dS[±M]</c> (for example <c>2d4+1</c>,
/// <c>d6</c>) or a pure constant <c>N</c>. Rolling reuses the ambient
/// <see cref="RandomUtils.Dice(int, int, int)" /> engine, so a fixed <see cref="BuiltInRng" />
/// seed yields reproducible sequences.
/// </summary>
/// <param name="Count">Number of dice; <c>0</c> for a pure constant.</param>
/// <param name="Sides">Sides per die; <c>0</c> for a pure constant.</param>
/// <param name="Modifier">Flat bonus added to (or, for a constant, equal to) the total.</param>
public readonly record struct DiceExpression(int Count, int Sides, int Modifier)
{
    private bool IsConstant => Count <= 0 || Sides <= 0;

    /// <summary>The lowest total this expression can produce (every die shows 1).</summary>
    public int Min => IsConstant ? Modifier : Count + Modifier;

    /// <summary>The highest total this expression can produce (every die shows its max face).</summary>
    public int Max => IsConstant ? Modifier : (Count * Sides) + Modifier;

    /// <summary>The expected (mean) total of this expression.</summary>
    public double Average => IsConstant ? Modifier : (Count * (Sides + 1) / 2.0) + Modifier;

    /// <summary>Rolls the expression, returning a random total in <c>[Min, Max]</c>.</summary>
    /// <returns>The rolled total, or <see cref="Modifier" /> for a pure constant.</returns>
    /// <remarks>Uses the ambient <see cref="BuiltInRng" />; for an injected source use <see cref="Roll(IRandom)" />.</remarks>
    public int Roll()
        => IsConstant ? Modifier : RandomUtils.Dice(Count, Sides, Modifier);

    /// <summary>Rolls the expression using the supplied random source, returning a total in <c>[Min, Max]</c>.</summary>
    /// <param name="random">The random source to draw from.</param>
    /// <returns>The rolled total, or <see cref="Modifier" /> for a pure constant.</returns>
    public int Roll(IRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (IsConstant)
        {
            return Modifier;
        }

        var total = Modifier;

        for (var i = 0; i < Count; i++)
        {
            total += random.NextInt(1, Sides + 1);
        }

        return total;
    }

    /// <summary>
    /// Parses dice notation such as <c>2d4+1</c>, <c>d6</c>, <c>5</c>, or the wrapped form
    /// <c>dice(2d4+1)</c>. Whitespace is ignored and the <c>d</c> separator is case-insensitive.
    /// </summary>
    /// <param name="input">The notation to parse.</param>
    /// <returns>The parsed <see cref="DiceExpression" />.</returns>
    /// <exception cref="FormatException">The input is not valid dice notation.</exception>
    public static DiceExpression Parse(string input)
    {
        if (TryParse(input, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid dice notation: '{input}'.");
    }

    /// <summary>Attempts to parse dice notation without throwing.</summary>
    /// <param name="input">The notation to parse; may be wrapped in <c>dice( ... )</c>.</param>
    /// <param name="result">The parsed expression on success; otherwise <c>default</c>.</param>
    /// <returns><c>true</c> if <paramref name="input" /> was valid dice notation.</returns>
    public static bool TryParse(string? input, out DiceExpression result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var text = input.Trim();

        if (text.StartsWith("dice(", StringComparison.OrdinalIgnoreCase) && text.EndsWith(')'))
        {
            text = text[5..^1];
        }

        text = text.Replace(" ", string.Empty, StringComparison.Ordinal);

        if (text.Length == 0)
        {
            return false;
        }

        var dIndex = text.IndexOf('d', StringComparison.OrdinalIgnoreCase);

        if (dIndex < 0)
        {
            if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var constant))
            {
                result = new DiceExpression(0, 0, constant);
                return true;
            }

            return false;
        }

        var countPart = text[..dIndex];
        var count = 1;

        if (countPart.Length > 0 &&
            !int.TryParse(countPart, NumberStyles.None, CultureInfo.InvariantCulture, out count))
        {
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        var rest = text[(dIndex + 1)..];

        if (rest.Length == 0)
        {
            return false;
        }

        var signIndex = rest.IndexOfAny(['+', '-']);
        var sidesPart = signIndex >= 0 ? rest[..signIndex] : rest;
        var modifier = 0;

        if (signIndex >= 0 &&
            !int.TryParse(rest[signIndex..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out modifier))
        {
            return false;
        }

        if (!int.TryParse(sidesPart, NumberStyles.None, CultureInfo.InvariantCulture, out var sides) || sides <= 0)
        {
            return false;
        }

        result = new DiceExpression(count, sides, modifier);
        return true;
    }
}
