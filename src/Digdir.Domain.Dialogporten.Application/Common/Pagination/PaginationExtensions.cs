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

        // TODO: Get continuation token from last item in the list
        var nextContinuationToken = items.Length > 0
            ? orderSet.GetContinuationToken(items[^1])
            : null;

        // TODO: Get order by from order set
        var orderBy = orderSet.GetOrderByAsString();

        // Compiling the timeSelector expression is a performance bottleneck.
        // We're currently caching the compiled expression giving an estimated
        // 40x performance boost over just calling Compile() directly. However
        // it's still a performance hit due to the ExpressionEqualityComparer.
        // If we need to squeeze out more performance we could restrict T to
        // be a ICreatableEntity and directly access the Timestamp property
        // eliminating the need for the timeSelector expression. This whould
        // increase the timestamp access by a factor of 800x at the cost of
        // cleaner code from the calling side. It would also restrict which
        // timestamp we can use as a pagination token. Pick your poison.
        //var nextContinuationToken = items.Length > 0
        //    ? orderSet.CompileOrGetCached().Invoke(items[^1])
        //    : null;

        return new PaginatedList<T>(items, hasNextPage, nextContinuationToken, orderBy);
    }

    //private static readonly ConcurrentDictionary<object, object> _compiledByExpression = new(comparer: new ExpressionEqualityComparer());
    //private static TFunc CompileOrGetCached<TFunc>(this Expression<TFunc> selectorExpression) where TFunc : Delegate => 
    //    (TFunc)_compiledByExpression.GetOrAdd(selectorExpression, x => ((Expression<TFunc>)x).Compile());
}
