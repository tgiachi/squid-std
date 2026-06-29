using System.Linq.Expressions;

namespace SquidStd.Tui.Internal;

/// <summary>Extracts member name, getter and setter from a member-access expression.</summary>
internal static class PropertyPath
{
    public static string NameOf<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new ArgumentException("Expression must be a direct property or field access.", nameof(expression));
    }

    public static Action<TSource, TValue> Setter<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
    {
        if (expression.Body is not MemberExpression member)
        {
            throw new ArgumentException("Expression must be a direct property or field access.", nameof(expression));
        }

        var targetParam = Expression.Parameter(typeof(TSource), "target");
        var valueParam = Expression.Parameter(typeof(TValue), "value");
        var memberAccess = Expression.MakeMemberAccess(targetParam, member.Member);
        var assign = Expression.Assign(memberAccess, valueParam);

        return Expression.Lambda<Action<TSource, TValue>>(assign, targetParam, valueParam).Compile();
    }
}
