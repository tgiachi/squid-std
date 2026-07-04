using System.Runtime.CompilerServices;
using SquidStd.Core.Utils;

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

    /// <summary>
    /// Returns a uniformly random element of the collection using <see cref="BuiltInRng" />.
    /// </summary>
    /// <exception cref="ArgumentException">The collection is empty.</exception>
    public static T RandomElement<T>(this IReadOnlyCollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be empty.", nameof(collection));
        }

        return collection.ElementAt(BuiltInRng.Next(0, collection.Count));
    }
}
