using System.Globalization;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogElement;

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
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(b => b
            .OperationId("GetDialogElementList")
        );

    }

    public override async Task HandleAsync(SearchDialogElementQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct));
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
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.Format("element list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.Format("get");
    }
}
