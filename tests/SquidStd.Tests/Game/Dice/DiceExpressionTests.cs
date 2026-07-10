using SquidStd.Core.Utils;
using SquidStd.Game.Dice;

namespace SquidStd.Tests.Game.Dice;

public class DiceExpressionTests
{
    [Theory]
    [InlineData("2d4", 2, 4, 0)]
    [InlineData("2d4+1", 2, 4, 1)]
    [InlineData("2d4-1", 2, 4, -1)]
    [InlineData("d6", 1, 6, 0)]
    [InlineData("D6", 1, 6, 0)]
    [InlineData(" 2 d 4 + 1 ", 2, 4, 1)]
    [InlineData("dice(2d4+1)", 2, 4, 1)]
    [InlineData("DICE(3d6-2)", 3, 6, -2)]
    public void Parse_ValidDiceNotation_ReturnsComponents(string input, int count, int sides, int modifier)
    {
        var expression = DiceExpression.Parse(input);

        Assert.Equal(count, expression.Count);
        Assert.Equal(sides, expression.Sides);
        Assert.Equal(modifier, expression.Modifier);
    }

    [Fact]
    public void Parse_PureConstant_HasZeroDice()
    {
        var expression = DiceExpression.Parse("5");

        Assert.Equal(0, expression.Count);
        Assert.Equal(0, expression.Sides);
        Assert.Equal(5, expression.Modifier);
    }

    [Theory]
    [InlineData("2d4", 2, 8, 5.0)]
    [InlineData("2d4+1", 3, 9, 6.0)]
    [InlineData("2d4-1", 1, 7, 4.0)]
    [InlineData("d6", 1, 6, 3.5)]
    public void MinMaxAverage_MatchExpectedBounds(string input, int min, int max, double average)
    {
        var expression = DiceExpression.Parse(input);

        Assert.Equal(min, expression.Min);
        Assert.Equal(max, expression.Max);
        Assert.Equal(average, expression.Average, 3);
    }

    [Fact]
    public void MinMaxAverage_Constant_AllEqualModifier()
    {
        var expression = DiceExpression.Parse("7");

        Assert.Equal(7, expression.Min);
        Assert.Equal(7, expression.Max);
        Assert.Equal(7.0, expression.Average, 3);
    }

    [Fact]
    public void Roll_StaysWithinMinMax()
    {
        var expression = DiceExpression.Parse("3d6+2");
        BuiltInRng.Reset(1234);

        for (var i = 0; i < 1000; i++)
        {
            var roll = expression.Roll();
            Assert.InRange(roll, expression.Min, expression.Max);
        }
    }

    [Fact]
    public void Roll_IsReproducibleWithSameSeed()
    {
        var expression = DiceExpression.Parse("2d20+3");

        BuiltInRng.Reset(42);
        var first = expression.Roll();
        BuiltInRng.Reset(42);
        var second = expression.Roll();

        Assert.Equal(first, second);
    }

    [Fact]
    public void Roll_Constant_ReturnsModifier()
    {
        var expression = DiceExpression.Parse("9");

        Assert.Equal(9, expression.Roll());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("2d")]
    [InlineData("d")]
    [InlineData("0d6")]
    [InlineData("2d0")]
    [InlineData("-1d6")]
    [InlineData("2d4+x")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(DiceExpression.TryParse(input, out _));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<FormatException>(() => DiceExpression.Parse("not-a-dice"));
    }
}
