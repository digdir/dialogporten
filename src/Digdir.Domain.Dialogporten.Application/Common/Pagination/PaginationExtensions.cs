using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

internal static class PaginationExtensions
{
    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        Expression<Func<TDestination, DateTimeOffset>> timeSelector,
        DateTimeOffset after,
        int pageSize,
        CancellationToken cancellationToken = default)
        => CreateAsync(queryable, timeSelector, after, pageSize, cancellationToken);

    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        Expression<Func<TDestination, DateTimeOffset>> timeSelector,
        IPaginationParameter parameter,
        CancellationToken cancellationToken = default)
        => CreateAsync(queryable, timeSelector, parameter.After!.Value, parameter.PageSize!.Value, cancellationToken);

    private static async Task<PaginatedList<T>> CreateAsync<T>(
        IQueryable<T> source,
        Expression<Func<T, DateTimeOffset>> timeSelector,
        DateTimeOffset after,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        const int NextOffset = 1;

        var greaterThanAfterPredicate = Expression.Lambda<Func<T, bool>>(
            Expression.GreaterThan(
                timeSelector.Body,
                Expression.Constant(after)),
            timeSelector.Parameters);

        var items = await source
            .OrderBy(timeSelector)
            .Where(greaterThanAfterPredicate)
            .Take(pageSize + NextOffset)
            .ToArrayAsync(cancellationToken);

        // Fetch one more item than requested to determine if there is a next page
        var hasNextPage = items.Length > pageSize;
        if (hasNextPage)
        {
            Array.Resize(ref items, pageSize);
        }

        // TODO: Compiling the timeSelector expression is a performance bottleneck.
        // Consider restricting creating PaginatedList only on ICreatableEntity
        // or find a way to compile the expression only once using a cache
        // or simply don't return the continuation token.
        var continuationToken = items.Length > 0
            ? timeSelector.Compile().Invoke(items[^1])
            : (DateTimeOffset?)null;

        return new PaginatedList<T>(items, pageSize, hasNextPage, continuationToken);
    }

    //public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
    //    this IQueryable<TDestination> queryable,
    //    DateTimeOffset after,
    //    int pageSize,
    //    CancellationToken cancellationToken = default)
    //    where TDestination : ICreatableEntity
    //    => CreateAsync(queryable, after, pageSize, cancellationToken);

    //public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
    //    this IQueryable<TDestination> queryable,
    //    IPaginationParameter parameter,
    //    CancellationToken cancellationToken = default)
    //    where TDestination : ICreatableEntity
    //    => CreateAsync(queryable, parameter.After!.Value, parameter.PageSize!.Value, cancellationToken);

    //private static async Task<PaginatedList<T>> CreateAsync<T>(
    //    IQueryable<T> source,
    //    DateTimeOffset after,
    //    int pageSize,
    //    CancellationToken cancellationToken = default)
    //    where T : ICreatableEntity
    //{
    //    if (source == null)
    //    {
    //        throw new ArgumentNullException(nameof(source));
    //    }

    //    const int NextOffset = 1;

    //    var items = await source
    //        .OrderBy(x => x.CreatedAt)
    //        .Where(x => x.CreatedAt > after)
    //        .Take(pageSize + NextOffset)
    //        .ToArrayAsync(cancellationToken);

    //    // Fetch one more item than requested to determine if there is a next page
    //    var hasNextPage = items.Length > pageSize;
    //    if (hasNextPage)
    //    {
    //        Array.Resize(ref items, pageSize);
    //    }

    //    var continuationToken = items.Length > 0
    //        ? items[^1].CreatedAt
    //        : (DateTimeOffset?)null;

    //    return new PaginatedList<T>(items, pageSize, hasNextPage, continuationToken);
    //}
}
