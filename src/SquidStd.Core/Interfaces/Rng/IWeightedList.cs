namespace SquidStd.Core.Interfaces.Rng;

/// <summary>
/// A mutable collection of items with associated positive weights, supporting weighted random
/// selection — useful for loot tables, spawn tables, and similar gameplay data.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IWeightedList<T>
{
    /// <summary>The number of items in the list.</summary>
    int Count { get; }

    /// <summary>The sum of all item weights.</summary>
    double TotalWeight { get; }

    /// <summary>Adds an item with the given weight.</summary>
    /// <param name="item">The item to add.</param>
    /// <param name="weight">The selection weight; must be positive.</param>
    void Add(T item, double weight);

    /// <summary>Selects an item at random, with probability proportional to its weight.</summary>
    /// <param name="random">The random source to draw from.</param>
    /// <returns>The selected item.</returns>
    T Next(IRandom random);
}
