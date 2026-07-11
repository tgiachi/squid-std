using SquidStd.Core.Interfaces.Rng;

namespace SquidStd.Core.Rng;

/// <summary>
/// A weighted collection backed by cumulative weights, selecting items in <c>O(log n)</c> via
/// binary search. Weights must be positive; selection probability is proportional to weight.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public sealed class WeightedList<T> : IWeightedList<T>
{
    private readonly List<T> _items = [];
    private readonly List<double> _cumulative = [];
    private double _total;

    public int Count => _items.Count;

    public double TotalWeight => _total;

    public void Add(T item, double weight)
    {
        if (weight <= 0 || double.IsNaN(weight) || double.IsInfinity(weight))
        {
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be a positive, finite number.");
        }

        _total += weight;
        _items.Add(item);
        _cumulative.Add(_total);
    }

    public T Next(IRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("The weighted list is empty.");
        }

        var roll = random.NextDouble() * _total;
        var index = _cumulative.BinarySearch(roll);

        if (index < 0)
        {
            index = ~index;
        }

        if (index >= _items.Count)
        {
            index = _items.Count - 1;
        }

        return _items[index];
    }
}
