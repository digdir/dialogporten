namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public sealed class PaginatedList<T>
{
    /// <summary>
    /// The paginated list of items
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// Whether there are more items available that can be fetched by supplying the continuation token
    /// </summary>
    public bool HasNextPage { get; }

    /// <summary>
    /// The continuation token to be used to fetch the next page of items
    /// </summary>
    /// <example>createdat_2024-07-31T09:09:03.0257090Z,id_0c089101-b7cf-a476-955c-f00a78d74a4e</example>
    public string? ContinuationToken { get; }

    /// <summary>
    /// The current sorting order of the items
    /// </summary>
    /// <example>createdat_desc,id_desc</example>
    public string OrderBy { get; }

    public PaginatedList(IEnumerable<T> items, bool hasNextPage, string? @continue, string orderBy)
    {
        ContinuationToken = @continue;
        HasNextPage = hasNextPage;
        OrderBy = orderBy;
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }
}
