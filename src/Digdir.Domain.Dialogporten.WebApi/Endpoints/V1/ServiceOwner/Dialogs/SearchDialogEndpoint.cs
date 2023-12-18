using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs;

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
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProviderSearch);

        RequestParam(p => p.ContinuationToken, "Supply \"continuationToken\" for the response to get the next page of results, if hasNextPage is true");
        RequestParam(p => p.Limit, $"Limit the number of results per page ({PaginationConstants.MinLimit}-{PaginationConstants.MaxLimit}, default: {PaginationConstants.DefaultLimit})");
    }
}
