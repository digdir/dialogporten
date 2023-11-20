using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.List;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogElement;

public class ListDialogElementEndpoint : Endpoint<ListDialogElementQuery>
{
    private readonly ISender _sender;

    public ListDialogElementEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/elements");
        Group<EndUserGroup>();

        Description(b => b
            .OperationId("GetDialogElementList")
        );

    }

    public override async Task HandleAsync(ListDialogElementQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct));
    }
}

public sealed class ListDialogElementEndpointSummary : Summary<ListDialogElementEndpoint, ListDialogElementQuery>
{
    public ListDialogElementEndpointSummary()
    {
        Summary = "Gets a list of dialog elements";
        Description = """
                Gets the list of elements belonging to a dialog
                """;
        Responses[StatusCodes.Status200OK] = string.Format(Constants.SwaggerSummary.ReturnedResult, "element list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status403Forbidden] = string.Format(Constants.SwaggerSummary.AccessDeniedToDialog, "get");
    }
}
