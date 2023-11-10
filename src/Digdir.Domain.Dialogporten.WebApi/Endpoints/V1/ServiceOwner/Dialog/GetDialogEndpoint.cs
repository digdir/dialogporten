using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public class GetDialogEndpoint : Endpoint<GetDialogQuery, GetDialogDto>
{
    private readonly ISender _sender;

    public GetDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}");
        Policies(AuthorizationPolicy.Serviceprovider);
        Group<ServiceOwnerGroup>();
        Description(b => b.OperationId("GetDialog"));
    }

    public override async Task HandleAsync(GetDialogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto =>
            {
                HttpContext.Response.Headers.ETag = dto.ETag.ToString();
                return SendOkAsync(dto, ct);
            },
            notFound => this.NotFoundAsync(notFound, ct));
    }
}

public sealed class GetDialogEndpointSummary : Summary<GetDialogEndpoint>
{
    public GetDialogEndpointSummary()
    {
        Summary = "Gets a single dialog";
        Description = """
                Gets a single dialog aggregate. For more information see the documentation (link TBD).
                """;
        Responses[StatusCodes.Status200OK] = "Successfully returned the dialog aggregate";
        Responses[StatusCodes.Status401Unauthorized] = Constants.SummaryErrorServiceOwner401;
        Responses[StatusCodes.Status403Forbidden] = "Unauthorized to get the supplied dialog (not owned by authenticated organization or has additional scope requirements defined in policy)";
        Responses[StatusCodes.Status404NotFound] = "The given dialog ID was not found or is deleted";
    }
}
