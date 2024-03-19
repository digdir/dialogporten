using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogActivities;

public class GetDialogActivityEndpoint : Endpoint<GetDialogActivityQuery, GetDialogActivityDto>
{
    private readonly ISender _sender;

    public GetDialogActivityEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/activities/{activityId}");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("GetDialogActivitySO")
            .ProducesOneOf(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound)
        );
    }

    public override async Task HandleAsync(GetDialogActivityQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct));
    }
}

public sealed class GetDialogActivityEndpointSummary : Summary<GetDialogActivityEndpoint>
{
    public GetDialogActivityEndpointSummary()
    {
        Summary = "Gets a single dialog activity";
        Description = """
                Gets a single activity belonging to a dialog. For more information see the documentation (link TBD).
                """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("activity");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogActivityNotFound;
    }
}
