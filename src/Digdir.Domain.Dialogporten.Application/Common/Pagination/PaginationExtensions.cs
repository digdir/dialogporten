using Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

internal static class PaginationExtensions
{
    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        IOrderSet<TDestination> orderSet,
        ContinuationToken? continuationToken,
        int limit,
        CancellationToken cancellationToken = default)
        => CreateAsync(
            queryable,
            orderSet,
            continuationToken,
            limit,
            cancellationToken);

    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        IOrderSet<TDestination> orderSet,
        IPaginationParameter parameter,
        CancellationToken cancellationToken = default)
        => CreateAsync(
            queryable,
            orderSet,
            parameter.Continue,
            parameter.Limit!.Value,
            cancellationToken);

    private static async Task<PaginatedList<T>> CreateAsync<T>(
        IQueryable<T> source,
        IOrderSet<T> orderSet,
        ContinuationToken? continuationToken,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        const int OneMore = 1;

        var items = await source
            .ApplyOrder(orderSet)
            .ApplyCondition(orderSet, continuationToken)
            .Take(limit + OneMore)
            .ToArrayAsync(cancellationToken);

        // Fetch one more item than requested to determine if there is a next page
        var hasNextPage = items.Length > limit;
        if (hasNextPage)
        {
            Array.Resize(ref items, limit);
        }

        var nextContinuationToken = items.Length > 0
            ? orderSet.GetContinuationToken(items[^1])
            : null;

        var orderBy = orderSet.GetOrderByAsString();

        return new PaginatedList<T>(items, hasNextPage, nextContinuationToken, orderBy);
    }
}
