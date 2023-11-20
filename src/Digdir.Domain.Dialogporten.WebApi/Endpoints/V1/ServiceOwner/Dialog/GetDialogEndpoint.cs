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

        Description(b => b
            .OperationId("GetDialogSO")
            .ProducesOneOf<GetDialogDto>(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound)
            );
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

                Note that this operation may return deleted dialogs (see the field `DeletedAt`).
                """;
        Responses[StatusCodes.Status200OK] = string.Format(Constants.SwaggerSummary.ReturnedResult, "aggregate");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = string.Format(Constants.SwaggerSummary.AccessDeniedToDialog, "get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
    }
}
