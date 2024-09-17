using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.NotificationCondition;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogActivities.NotificationCondition;

public sealed class NotificationConditionEndpoint : Endpoint<NotificationConditionQuery>
{
    private readonly ISender _sender;

    public NotificationConditionEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/notification-condition");
        Policies(AuthorizationPolicy.NotificationConditionCheck);
        Group<ServiceOwnerGroup>();

        Description(b => NotificationConditionSwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(NotificationConditionQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            notFound => this.NotFoundAsync(notFound, ct));
    }
}
