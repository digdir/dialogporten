using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElements;

public class SearchDialogElementEndpoint : Endpoint<SearchDialogElementQuery>
{
    private readonly ISender _sender;

    public SearchDialogElementEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/elements");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b.
            OperationId("GetDialogElementListSO")
        );
    }

    public override async Task HandleAsync(SearchDialogElementQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct));
    }
}

public sealed class SearchDialogElementEndpointSummary : Summary<SearchDialogElementEndpoint, SearchDialogElementQuery>
{
    public SearchDialogElementEndpointSummary()
    {
        Summary = "Gets a list of dialog elements";
        Description = """
                Gets the list of elements belonging to a dialog
                """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("element list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("get");
    }
}
