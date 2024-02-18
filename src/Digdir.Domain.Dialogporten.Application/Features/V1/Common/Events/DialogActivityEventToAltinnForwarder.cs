using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogActivityEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogActivityCreatedDomainEvent>
{
    public DialogActivityEventToAltinnForwarder(ICloudEventBus cloudEventBus, IOptions<ApplicationSettings> settings)
        : base(cloudEventBus, settings) { }

    public async Task Handle(DialogActivityCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent.TypeId),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{DialogportenBaseUrl()}/api/v1/enduser/dialogs/{domainEvent.DialogId}/activities/{domainEvent.ActivityId}",
            Data = GetCloudEventData(domainEvent)
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    private static Dictionary<string, object> GetCloudEventData(DialogActivityCreatedDomainEvent domainEvent)
    {
        var data = new Dictionary<string, object>
        {
            ["activityId"] = domainEvent.ActivityId.ToString(),
        };

        if (domainEvent.ExtendedType is not null)
        {
            data["extendedActivityType"] = domainEvent.ExtendedType;
        }

        if (domainEvent.RelatedActivityId is not null)
        {
            data["relatedActivityId"] = domainEvent.RelatedActivityId.ToString()!;
        }

        if (domainEvent.DialogElementId is not null)
        {
            data["dialogElementId"] = domainEvent.DialogElementId.ToString()!;
        }

        if (domainEvent.DialogElementType is not null)
        {
            data["dialogElementType"] = domainEvent.DialogElementType.ToString();
        }

        return data;
    }
}
