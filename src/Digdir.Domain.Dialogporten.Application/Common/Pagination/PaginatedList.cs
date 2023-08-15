namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public sealed class PaginatedList<T>
{
    public List<T> Items { get; }
    public DateTimeOffset? Continue { get; }
    public bool HasNextPage { get; }

    public PaginatedList(IEnumerable<T> items, bool hasNextPage, DateTimeOffset? @continue)
    {
        Continue = @continue;
        HasNextPage = hasNextPage;
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }
}
