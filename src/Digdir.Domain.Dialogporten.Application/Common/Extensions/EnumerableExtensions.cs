namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal delegate Task<IEnumerable<TDestination>> CreatesDelegade<TDestination, in TSource>(IEnumerable<TSource> creatables, CancellationToken cancellationToken = default);
internal delegate Task UpdatesDelegade<in TDestination, in TSource>(IEnumerable<IUpdateSet<TDestination, TSource>> updateSets, CancellationToken cancellationToken = default);
internal delegate Task DeletesDelegade<in TDestination>(IEnumerable<TDestination> deletables, CancellationToken cancellationToken = default);

internal static class EnumerableExtensions
{
    public static async Task<List<TDestination>> MergeAsync<TDestination, TSource, TKey>(
        this IEnumerable<TDestination> destinations,
        IEnumerable<TSource> sources,
        Func<TDestination, TKey> destinationKeySelector,
        Func<TSource, TKey> sourceKeySelector,
        CreatesDelegade<TDestination, TSource>? create = null,
        UpdatesDelegade<TDestination, TSource>? update = null,
        DeletesDelegade<TDestination>? delete = null,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(destinations);
        ArgumentNullException.ThrowIfNull(sourceKeySelector);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        comparer ??= EqualityComparer<TKey>.Default;

        // Ensure concrete incoming enumerables
        destinations = destinations is List<TDestination> ? destinations : destinations.ToList();
        sources = sources is List<TSource> ? sources : sources.ToList();

        // Assert no duplicate non default keys
        sources.AssertNoDuplicateNonDefaultKeys(sourceKeySelector, comparer);
        destinations.AssertNoDuplicateNonDefaultKeys(destinationKeySelector, comparer);

        // Calculate merge lists
        var updates = destinations
            .Join(sources,
                destinationKeySelector,
                sourceKeySelector,
                (destination, source) => (IUpdateSet<TDestination, TSource>)new UpdateSet<TDestination, TSource> 
                    { 
                        Destination = destination, 
                        Source = source 
                    },
                comparer)
            .ToList();

        var result = destinations;
        if (create is not null)
        {
            var creates = sources
                .Except(updates.Select(x => x.Source))
                .ToList();
            result = result.Concat(await create(creates, cancellationToken));
        }

        if (update is not null)
        {
            await update(updates, cancellationToken);
        }

        if (delete is not null)
        {
            var deleates = destinations
                .Except(updates.Select(x => x.Destination))
                .ToList();
            await delete(deleates, cancellationToken);
            result = result.Except(deleates);
        }

        return result.ToList();
    }

    public static IEnumerable<(TInner Dependent, TOuter Principal)> JoinPairs<TInner, TOuter, TInnerKey, TOuterKey>(
        this IEnumerable<KeyValuePair<TInnerKey, TOuterKey>> keyPairs,
        IEnumerable<TInner> inner,
        IEnumerable<TOuter> outer,
        Func<TInner, TInnerKey> innerKeySelector,
        Func<TOuter, TOuterKey> outerKeySelector)
        where TInner : notnull
        where TOuter : notnull
        where TInnerKey : notnull
        where TOuterKey : notnull
    {
        // Ensure concrete incoming enumerables
        keyPairs = keyPairs 
            is Dictionary<TInnerKey, TOuterKey> 
            or List<KeyValuePair<TInnerKey, TOuterKey>> 
            ? keyPairs 
            : keyPairs.ToList();
        inner = inner is List<TInner> ? inner : inner.ToList();
        outer = outer is List<TOuter> ? outer : outer.ToList();

        // TODO: keyPairs should be different
        // TODO: keyPairs should match with inner and outer exacly once
        // Assert no duplicate non default keys
        inner.AssertNoDuplicateNonDefaultKeys(innerKeySelector);
        outer.AssertNoDuplicateNonDefaultKeys(outerKeySelector);

        return keyPairs
            .Join(inner,
                outerKeySelector: x => x.Key,
                innerKeySelector: innerKeySelector,
                resultSelector: (x, dependent) => (Dependent: dependent, PrincipalId: x.Value))
            .Join(outer,
                outerKeySelector: x => x.PrincipalId,
                innerKeySelector: outerKeySelector,
                resultSelector: (x, principal) => (Dependent: x.Dependent, Principal: principal));
    }

    private static void AssertNoDuplicateNonDefaultKeys<T, TKey>(
        this IEnumerable<T> values,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        comparer ??= EqualityComparer<TKey>.Default;

        var duplicateKeys = values
            .Select(keySelector)
            .GroupBy(x => x, comparer)
            .Where(x => !comparer.Equals(x.Key, default) && x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Any())
        {
            var typename = typeof(T).Name;
            throw new InvalidOperationException(
                $"Expected elements with unique non default keys. The following duplicate/default " +
                $"keys were detected for {typename}: [{string.Join(",", duplicateKeys)}].");
        }
    }

    private readonly struct UpdateSet<TDestination, TSource> : IUpdateSet<TDestination, TSource>
    {
        public required TDestination Destination { get; init; }
        public required TSource Source { get; init; }
    }
}

internal interface IUpdateSet<out TDestination, out TSource>
{
    TDestination Destination { get; }
    TSource Source { get; }
}
