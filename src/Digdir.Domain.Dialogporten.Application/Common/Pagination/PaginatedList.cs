//using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public sealed class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex < TotalPages - 1;

    public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }
}
