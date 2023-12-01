using System.Globalization;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public class SearchDialogEndpoint : Endpoint<SearchDialogQuery, PaginatedList<SearchDialogDto>>
{
    private readonly ISender _sender;

    public SearchDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs");
        Policies(AuthorizationPolicy.ServiceProviderSearch);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("GetDialogListSO")
            .ClearDefaultProduces(StatusCodes.Status403Forbidden)
        );
    }

    public override async Task HandleAsync(SearchDialogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            paginatedDto => SendOkAsync(paginatedDto, ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}

public sealed class ListDialogEndpointSummary : Summary<SearchDialogEndpoint, SearchDialogQuery>
{
    public ListDialogEndpointSummary()
    {
        Summary = "Gets a list of dialogs";
        Description = """
                Performs a search for dialogs, returning a paginated list of dialogs. For more information see the documentation (link TBD).

                * All date parameters must contain explicit time zone. Example: 2023-10-27T10:00:00Z or 2023-10-27T10:00:00+01:00
                * See "continuationToken" in the response for how to get the next page of results.
                * hasNextPage will be set to true if there are more items to get.
                """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.Format("list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure;

        RequestParam(p => p.ServiceResource, "Filter by one or more service resources");
        RequestParam(p => p.Party, "Filter by one or more owning parties");
        RequestParam(p => p.ExtendedStatus, "Filter by one or more extended statuses");
        RequestParam(p => p.Status, "Filter by status");
        RequestParam(p => p.CreatedAfter, "Only return dialogs created after this date");
        RequestParam(p => p.CreatedBefore, "Only return dialogs created before this date");
        RequestParam(p => p.UpdatedAfter, "Only return dialogs updated after this date");
        RequestParam(p => p.UpdatedBefore, "Only return dialogs updated before this date");
        RequestParam(p => p.DueAfter, "Only return dialogs with due date after this date");
        RequestParam(p => p.DueBefore, "Only return dialogs with due date before this date");
        RequestParam(p => p.VisibleAfter, "Only return dialogs with visible-from date after this date");
        RequestParam(p => p.VisibleBefore, "Only return dialogs with visible-from date before this date");
        RequestParam(p => p.Search, "Search string for free text search. Will attempt to fuzzily match in all free text fields in the aggregate");
        RequestParam(p => p.SearchCultureCode, "Limit free text search to texts with this culture code, e.g. \"nb-NO\". Default: search all culture codes");
        RequestParam(p => p.OrderBy, "Order by one or more fields with ascending or descending direction. Example: dueAt_asc,updatedAt_desc");
        RequestParam(p => p.ContinuationToken, "Supply \"continuationToken\" for the response to get the next page of results, if hasNextPage is true");
        RequestParam(p => p.Limit, $"Limit the number of results per page ({PaginationConstants.MinLimit}-{PaginationConstants.MaxLimit}, default: {PaginationConstants.DefaultLimit})");
    }
}
