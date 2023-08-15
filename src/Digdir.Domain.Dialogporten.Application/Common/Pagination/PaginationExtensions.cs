using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

internal static class PaginationExtensions
{
    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        Expression<Func<TDestination, DateTimeOffset?>> paginationToken,
        DateTimeOffset continuationToken,
        int limit,
        OrderDirection orderDirection = OrderDirection.Asc,
        CancellationToken cancellationToken = default)
        => CreateAsync(
            queryable,
            paginationToken,
            continuationToken,
            limit,
            orderDirection,
            cancellationToken);

    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        Expression<Func<TDestination, DateTimeOffset?>> paginationToken,
        IPaginationParameter parameter,
        CancellationToken cancellationToken = default)
        => CreateAsync(
            queryable, 
            paginationToken, 
            parameter.Continue!.Value, 
            parameter.Limit!.Value, 
            parameter.Direction!.Value, 
            cancellationToken);

    private static async Task<PaginatedList<T>> CreateAsync<T>(
        IQueryable<T> source,
        Expression<Func<T, DateTimeOffset?>> paginationToken,
        DateTimeOffset continuationToken,
        int limit,
        OrderDirection orderDirection = OrderDirection.Asc,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        const int OneMore = 1;
        var queryPredicte = BuildQueryPredicate(paginationToken, continuationToken, orderDirection);
        var orderedQuery = orderDirection switch
        {
            OrderDirection.Asc => source.OrderBy(paginationToken),
            OrderDirection.Desc => source.OrderByDescending(paginationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(orderDirection), orderDirection, null)
        };
        
        var items = await orderedQuery
            .Where(queryPredicte)
            .Take(limit + OneMore)
            .ToArrayAsync(cancellationToken);

        // Fetch one more item than requested to determine if there is a next page
        var hasNextPage = items.Length > limit;
        if (hasNextPage)
        {
            Array.Resize(ref items, limit);
        }

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
        var nextContinuationToken = items.Length > 0
            ? paginationToken.CompileOrGetCached().Invoke(items[^1])
            : null;

        return new PaginatedList<T>(items, hasNextPage, nextContinuationToken);
    }

    private static Expression<Func<T, bool>> BuildQueryPredicate<T>(
        Expression<Func<T, DateTimeOffset?>> paginationToken,
        DateTimeOffset continuationToken,
        OrderDirection orderDirection)
    {
        var continuationConstant = Expression.Constant(continuationToken, typeof(DateTimeOffset?));
        var orderCondition = orderDirection switch
        {
            OrderDirection.Asc => Expression.GreaterThan(paginationToken.Body, continuationConstant),
            OrderDirection.Desc => Expression.LessThan(paginationToken.Body, continuationConstant),
            _ => throw new ArgumentOutOfRangeException(nameof(orderDirection), orderDirection, null)
        };
        var notNullCondition = Expression.NotEqual(paginationToken.Body, Expression.Constant(null));
        var notNullAndOrderCondition = Expression.AndAlso(notNullCondition, orderCondition);
        var queryPredicte = Expression.Lambda<Func<T, bool>>(notNullAndOrderCondition, paginationToken.Parameters);
        return queryPredicte;
    }

    private static readonly ConcurrentDictionary<object, object> _compiledByExpression = new(comparer: new ExpressionEqualityComparer());
    private static TFunc CompileOrGetCached<TFunc>(this Expression<TFunc> selectorExpression) where TFunc : Delegate => 
        (TFunc)_compiledByExpression.GetOrAdd(selectorExpression, x => ((Expression<TFunc>)x).Compile());
}
