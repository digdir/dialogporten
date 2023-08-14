namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public sealed class PaginatedList<T>
{
    public List<T> Items { get; }
    public DateTimeOffset? Continue { get; }
    public int PageSize { get; }
    public bool HasNextPage { get; }

    public PaginatedList(IEnumerable<T> items, int pageSize, bool hasNextPage, DateTimeOffset? @continue)
    {
        Continue = @continue;
        PageSize = pageSize;
        HasNextPage = hasNextPage;
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }
}
