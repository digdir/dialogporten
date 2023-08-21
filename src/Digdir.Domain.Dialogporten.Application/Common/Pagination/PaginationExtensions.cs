using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
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

        // Compiling the timeSelector expression is a performance bottleneck.
        // We're currently caching the compiled expression giving an estimated
        // 40x performance boost over just calling Compile() directly. However
        // it's still a performance hit due to the ExpressionEqualityComparer.
        // If we need to squeeze out more performance we could restrict T to
        // be a ICreatableEntity and directly access the Timestamp property
        // eliminating the need for the timeSelector expression. This would
        // increase the timestamp access by a factor of 800x at the cost of
        // cleaner code from the calling side. Pick your poison.
        var continuationToken = items.Length > 0
            ? timeSelector.CompileOrGetCached().Invoke(items[^1])
            : (DateTimeOffset?)null;

        return new PaginatedList<T>(items, pageSize, hasNextPage, continuationToken);
    }

    private static readonly ConcurrentDictionary<object, object> _compiledByExpression = new(comparer: new ExpressionEqualityComparer());
    private static TFunc CompileOrGetCached<TFunc>(this Expression<TFunc> selectorExpression) where TFunc : Delegate => 
        (TFunc)_compiledByExpression.GetOrAdd(selectorExpression, x => ((Expression<TFunc>)x).Compile());
}
