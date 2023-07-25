namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal delegate Task<IEnumerable<TDestination>> CreatesDelegade<TDestination, in TSource>(IEnumerable<TSource> creatables, CancellationToken cancellationToken = default);
internal delegate Task UpdatesDelegade<TDestination, TSource>(IEnumerable<IUpdateSet<TDestination, TSource>> updateSets, CancellationToken cancellationToken = default);
internal delegate Task DeletesDelegade<in TDestination>(IEnumerable<TDestination> deletables, CancellationToken cancellationToken = default);

internal static class DeletesDelegade
{
    public static Task NoOp<TDestination>(IEnumerable<TDestination> deletables, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal static class EnumerableExtensions
{
    public static async Task<List<TDestination>> PureMergeAsync<TDestination, TSource, TKey>(
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
            if (creates.Any())
            {
                result = result.Concat(await create(creates, cancellationToken));
            }
        }

        if (update is not null && updates.Any())
        {
            await update(updates, cancellationToken);
        }

        if (delete is not null)
        {
            var deleates = destinations
                .Except(updates.Select(x => x.Destination))
                .ToList();
            if (deleates.Any())
            {
                await delete(deleates, cancellationToken);
                result = result.Except(deleates);
            }
        }

        return result.ToList();
    }

    public static async Task MergeAsync<TDestination, TSource, TKey>(
        this ICollection<TDestination> destinations,
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

        await Delete(destinations, delete, updates, cancellationToken);
        await Update(update, updates, cancellationToken);
        await Create(destinations, sources, create, updates, cancellationToken);
    }

    private static async Task Create<TDestination, TSource>(
        ICollection<TDestination> destinations,
        IEnumerable<TSource> sources,
        CreatesDelegade<TDestination, TSource>? create,
        List<IUpdateSet<TDestination, TSource>> updates,
        CancellationToken cancellationToken)
    {
        if (create is null)
        {
            return;
        }

        var creates = sources
            .Except(updates.Select(x => x.Source))
            .ToList();

        if (creates.Count == 0)
        {
            return;
        }

        destinations.AddRange(await create(creates, cancellationToken));
    }

    private static async Task Update<TDestination, TSource>(
        UpdatesDelegade<TDestination, TSource>? update,
        List<IUpdateSet<TDestination, TSource>> updates,
        CancellationToken cancellationToken)
    {
        if (update is null || updates.Count == 0)
        {
            return;
        }
        await update(updates, cancellationToken);
    }

    private static async Task Delete<TDestination, TSource>(
        ICollection<TDestination> destinations,
        DeletesDelegade<TDestination>? delete,
        List<IUpdateSet<TDestination, TSource>> updates,
        CancellationToken cancellationToken)
    {
        if (delete is null)
        {
            return;
        }

        var deleates = destinations
            .Except(updates.Select(x => x.Destination))
            .ToList();

        if (deleates.Count == 0)
        {
            return;
        }

        await delete(deleates, cancellationToken);
        foreach (var item in deleates)
        {
            destinations.Remove(item);
        }
    }

    private static void AddRange<T>(
        this ICollection<T> destination,
        IEnumerable<T> source)
    {
        if (destination is List<T> list)
        {
            list.AddRange(source);
            return;
        }

        foreach (var item in source)
        {
            destination.Add(item);
        }
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

        public void Deconstruct(out TSource source, out TDestination destination)
        {
            source = Source;
            destination = Destination;
        }
    }
}

internal interface IUpdateSet<TDestination, TSource>
{
    TDestination Destination { get; }
    TSource Source { get; }
    void Deconstruct(out TSource source, out TDestination destination);
}
