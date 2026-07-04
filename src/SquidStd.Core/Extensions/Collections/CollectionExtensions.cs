using System.Runtime.CompilerServices;

namespace SquidStd.Core.Extensions.Collections;

/// <summary>
/// Small quality-of-life extensions for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Adds <paramref name="item" /> to the collection only when it is not null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddNotNull<T>(this ICollection<T> collection, T? item)
        where T : class
    {
        if (item is not null)
        {
            collection.Add(item);
        }
    }
}
