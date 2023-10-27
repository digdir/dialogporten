namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public sealed class PaginatedList<T>
{
    public List<T> Items { get; }
    public bool HasNextPage { get; }
    public string? ContinuationToken { get; }
    public string OrderBy { get; }

    public PaginatedList(IEnumerable<T> items, bool hasNextPage, string? @continue, string orderBy)
    {
        ContinuationToken = @continue;
        HasNextPage = hasNextPage;
        OrderBy = orderBy;
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }
}
