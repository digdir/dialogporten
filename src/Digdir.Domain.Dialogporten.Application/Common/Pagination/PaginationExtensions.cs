using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

internal static class PaginationExtensions
{
    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        => CreateAsync(queryable, pageIndex, pageSize, cancellationToken);
    public static Task<PaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, IPaginationParameter parameter, CancellationToken cancellationToken = default)
        => CreateAsync(queryable, parameter.PageIndex, parameter.PageSize, cancellationToken);

    public static PaginatedList<TDestination> ToPaginatedList<TDestination>(this IEnumerable<TDestination> enumerable, int pageIndex, int pageSize)
        => Create(enumerable, pageIndex, pageSize);
    public static PaginatedList<TDestination> ToPaginatedList<TDestination>(this IEnumerable<TDestination> enumerable, IPaginationParameter parameter)
        => Create(enumerable, parameter.PageIndex, parameter.PageSize);

    private static async Task<PaginatedList<T>> CreateAsync<T>(IQueryable<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var count = await source
            .CountAsync(cancellationToken);
        var items = await source
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    private static PaginatedList<T> Create<T>(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var sourceList = source.ToList();
        var count = sourceList.Count;
        var items = sourceList
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
