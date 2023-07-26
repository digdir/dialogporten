using AutoMapper;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerable;

internal delegate Task<IEnumerable<TDestination>> CreateAsyncDelegade<TDestination, in TSource>(IEnumerable<TSource> creatables, CancellationToken cancellationToken = default);
internal delegate Task UpdateAsyncDelegade<TDestination, TSource>(IEnumerable<UpdateSet<TDestination, TSource>> updateSets, CancellationToken cancellationToken = default);
internal delegate Task DeleteAsyncDelegade<in TDestination>(IEnumerable<TDestination> deletables, CancellationToken cancellationToken = default);

internal delegate IEnumerable<TDestination> CreateDelegade<TDestination, in TSource>(IEnumerable<TSource> creatables);
internal delegate void UpdateDelegade<TDestination, TSource>(IEnumerable<UpdateSet<TDestination, TSource>> updateSets);
internal delegate void DeleteDelegade<in TDestination>(IEnumerable<TDestination> deletables);

internal static class DeleteDelegade
{
    public static Task NoOp<TDestination>(IEnumerable<TDestination> deletables, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public static void NoOp<TDestination>(IEnumerable<TDestination> deletables) { }
}

internal static class MergeExtensions
{
    public static void Merge<TDestination, TSource, TKey>(
        this ICollection<TDestination> destinations,
        IEnumerable<TSource> sources,
        Func<TDestination, TKey> destinationKeySelector,
        Func<TSource, TKey> sourceKeySelector,
        CreateDelegade<TDestination, TSource>? create = null,
        UpdateDelegade<TDestination, TSource>? update = null,
        DeleteDelegade<TDestination>? delete = null,
        IEqualityComparer<TKey>? comparer = null)
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
        var updates = GetUpdateSets(destinations, sources, destinationKeySelector, sourceKeySelector, comparer);

        Delete(destinations, delete, updates);
        Update(update, updates);
        Create(destinations, sources, create, updates);
    }

    public static async Task MergeAsync<TDestination, TSource, TKey>(
        this ICollection<TDestination> destinations,
        IEnumerable<TSource> sources,
        Func<TDestination, TKey> destinationKeySelector,
        Func<TSource, TKey> sourceKeySelector,
        CreateAsyncDelegade<TDestination, TSource>? create = null,
        UpdateAsyncDelegade<TDestination, TSource>? update = null,
        DeleteAsyncDelegade<TDestination>? delete = null,
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
        var updates = GetUpdateSets(destinations, sources, destinationKeySelector, sourceKeySelector, comparer);

        await DeleteAsync(destinations, delete, updates, cancellationToken);
        await UpdateAsync(update, updates, cancellationToken);
        await CreateAsync(destinations, sources, create, updates, cancellationToken);
    }

    private static async Task CreateAsync<TDestination, TSource>(
        ICollection<TDestination> destinations,
        IEnumerable<TSource> sources,
        CreateAsyncDelegade<TDestination, TSource>? create,
        List<UpdateSet<TDestination, TSource>> updates,
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

    private static async Task UpdateAsync<TDestination, TSource>(
        UpdateAsyncDelegade<TDestination, TSource>? update,
        List<UpdateSet<TDestination, TSource>> updates,
        CancellationToken cancellationToken)
    {
        if (update is null || updates.Count == 0)
        {
            return;
        }
        await update(updates, cancellationToken);
    }

    private static async Task DeleteAsync<TDestination, TSource>(
        ICollection<TDestination> destinations,
        DeleteAsyncDelegade<TDestination>? delete,
        List<UpdateSet<TDestination, TSource>> updates,
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

    private static void Create<TDestination, TSource>(
        ICollection<TDestination> destinations,
        IEnumerable<TSource> sources,
        CreateDelegade<TDestination, TSource>? create,
        List<UpdateSet<TDestination, TSource>> updates)
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

        destinations.AddRange(create(creates));
    }

    private static void Update<TDestination, TSource>(
        UpdateDelegade<TDestination, TSource>? update,
        List<UpdateSet<TDestination, TSource>> updates)
    {
        if (update is null || updates.Count == 0)
        {
            return;
        }
        update(updates);
    }

    private static void Delete<TDestination, TSource>(
        ICollection<TDestination> destinations,
        DeleteDelegade<TDestination>? delete,
        List<UpdateSet<TDestination, TSource>> updates)
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

        delete(deleates);
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

    private static List<UpdateSet<TDestination, TSource>> GetUpdateSets<TDestination, TSource, TKey>(
        ICollection<TDestination> destinations,
        IEnumerable<TSource> sources,
        Func<TDestination, TKey> destinationKeySelector,
        Func<TSource, TKey> sourceKeySelector,
        IEqualityComparer<TKey>? comparer)
    {
        return destinations.Join(sources,
                destinationKeySelector,
                sourceKeySelector,
                (destination, source) => new UpdateSet<TDestination, TSource>(destination, source),
                comparer)
            .ToList();
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

    internal static void Update<TDestination, TSource>(this IMapper mapper, IEnumerable<UpdateSet<TDestination, TSource>> updateSets)
    {
        foreach (var (source, destination) in updateSets)
        {
            mapper.Map(source, destination);
        }
    }
}
