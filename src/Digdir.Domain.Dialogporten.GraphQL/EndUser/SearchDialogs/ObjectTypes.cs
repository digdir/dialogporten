using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;

[InterfaceType("SearchDialogError")]
public interface ISearchDialogError
{
    public string Message { get; set; }
}

public sealed class SearchDialogForbidden : ISearchDialogError
{
    public string Message { get; set; } = "Forbidden";
}

public sealed class SearchDialogValidationError : ISearchDialogError
{
    public string Message { get; set; } = null!;
}

public sealed class SearchDialogsPayload
{
    public List<SearchDialog>? Items { get; init; }
    public bool HasNextPage { get; }
    public string? ContinuationToken { get; }
    public string OrderBy { get; } = null!;
    public List<ISearchDialogError> Errors { get; init; } = [];
}

public sealed class SearchDialog
{
    public Guid Id { get; init; }
    public string Org { get; init; } = null!;
    public string ServiceResource { get; init; } = null!;
    public string Party { get; init; } = null!;
    public int? Progress { get; init; }
    public int? GuiAttachmentCount { get; init; }
    public string? ExtendedStatus { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? DueAt { get; init; }

    public DialogStatus Status { get; init; }

    public Activity? LatestActivity { get; init; }

    public List<Content> Content { get; init; } = [];
    public List<SeenLog> SeenSinceLastUpdate { get; init; } = [];
}

public sealed class SearchDialogInput
{
    [GraphQLDescription("Filter by one or more service owner codes")]
    public List<string>? Org { get; init; }

    [GraphQLDescription("Filter by one or more service resources")]
    public List<string>? ServiceResource { get; init; }

    [GraphQLDescription("Filter by one or more owning parties")]
    public List<string>? Party { get; init; }

    [GraphQLDescription("Filter by one or more extended statuses")]
    public List<string>? ExtendedStatus { get; init; }

    [GraphQLDescription("Filter by external reference")]
    public string? ExternalReference { get; init; }

    [GraphQLDescription("Filter by status")]
    public List<DialogStatus>? Status { get; init; }

    [GraphQLDescription("Only return dialogs created after this date")]
    public DateTimeOffset? CreatedAfter { get; init; }

    [GraphQLDescription("Only return dialogs created before this date")]
    public DateTimeOffset? CreatedBefore { get; init; }

    [GraphQLDescription("Only return dialogs updated after this date")]
    public DateTimeOffset? UpdatedAfter { get; init; }

    [GraphQLDescription("Only return dialogs updated before this date")]
    public DateTimeOffset? UpdatedBefore { get; init; }

    [GraphQLDescription("Only return dialogs with due date after this date")]
    public DateTimeOffset? DueAfter { get; init; }

    [GraphQLDescription("Only return dialogs with due date before this date")]
    public DateTimeOffset? DueBefore { get; init; }

    [GraphQLDescription("Search string for free text search. Will attempt to fuzzily match in all free text fields in the aggregate")]
    public string? Search { get; init; }

    [GraphQLDescription("Limit free text search to texts with this culture code, e.g. \"nb-NO\". Default: search all culture codes")]
    public string? SearchCultureCode { get; init; }
}
