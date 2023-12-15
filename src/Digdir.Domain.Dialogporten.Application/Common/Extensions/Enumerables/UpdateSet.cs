namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;

internal readonly struct UpdateSet<TDestination, TSource>
{
    public TDestination Destination { get; init; }
    public TSource Source { get; init; }

    public UpdateSet(TDestination destination, TSource source)
    {
        Destination = destination;
        Source = source;
    }

    public void Deconstruct(out TSource source, out TDestination destination)
    {
        source = Source;
        destination = Destination;
    }

    public static List<UpdateSet<TDestination, TSource>> Create<TKey>(
        ICollection<TDestination> destinations,
        ICollection<TSource> sources,
        Func<TDestination, TKey> destinationKeySelector,
        Func<TSource, TKey> sourceKeySelector,
        IEqualityComparer<TKey> comparer)
    {
        AssertNoDuplicateNonDefaultKeys(destinations, destinationKeySelector, comparer);
        AssertNoDuplicateNonDefaultKeys(sources, sourceKeySelector, comparer);
        return destinations.Join(sources,
                destinationKeySelector,
                sourceKeySelector,
                (destination, source) => new UpdateSet<TDestination, TSource>(destination, source),
                comparer)
            .ToList();
    }

    private static void AssertNoDuplicateNonDefaultKeys<T, TKey>(
        IEnumerable<T> values,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey> comparer)
    {
        var duplicateKeys = values
            .Select(keySelector)
            .GroupBy(x => x, comparer)
            .Where(x => !comparer.Equals(x.Key, default) && x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Count != 0)
        {
            var typename = typeof(T).Name;
            throw new InvalidOperationException(
                $"Expected elements with unique non default keys. The following duplicate/default " +
                $"keys were detected for {typename}: [{string.Join(",", duplicateKeys)}].");
        }
    }
}
