namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal delegate Task<IEnumerable<TDestination>> CreatesDelegade<TDestination, TSource>(IEnumerable<TSource> creatables, CancellationToken cancellationToken = default);
internal delegate Task UpdatesDelegade<TDestination, TSource>(IEnumerable<UpdateSet<TDestination, TSource>> updateSets, CancellationToken cancellationToken = default);
internal delegate Task DeletesDelegade<TDestination>(IEnumerable<TDestination> deletables, CancellationToken cancellationToken = default);

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
                (destination, source) => new UpdateSet<TDestination, TSource> { Destination = destination, Source = source },
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

    private static void AssertNoDuplicateNonDefaultKeys<T, TKey>(
        this IEnumerable<T> values,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey> comparer)
    {
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
}

internal readonly struct UpdateSet<TDestination, TSource>
{
    public required TDestination Destination { get; init; }
    public required TSource Source { get; init; }
}