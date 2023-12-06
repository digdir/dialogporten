using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogElement;

public class GetDialogElementEndpoint : Endpoint<GetDialogElementQuery>
{
    private readonly ISender _sender;

    public GetDialogElementEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/elements/{elementId}");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(b => b
            .OperationId("GetDialogElement")
            .ProducesOneOf(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound)
        );
    }

    public override async Task HandleAsync(GetDialogElementQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct));
    }
}

public sealed class GetDialogElementEndpointSummary : Summary<GetDialogElementEndpoint>
{
    public GetDialogElementEndpointSummary()
    {
        Summary = "Gets a single dialog element";
        Description = """
                Gets a single element belonging to a dialog. For more information see the documentation (link TBD).
                """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.Format("element");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.Format("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogElementNotFound;
    }
}
