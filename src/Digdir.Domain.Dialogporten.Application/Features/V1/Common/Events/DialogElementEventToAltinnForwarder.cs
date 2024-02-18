using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogElementEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogElementUpdatedDomainEvent>,
    INotificationHandler<DialogElementCreatedDomainEvent>,
    INotificationHandler<DialogElementDeletedDomainEvent>
{
    public DialogElementEventToAltinnForwarder(ICloudEventBus cloudEventBus, IOptions<ApplicationSettings> settings)
        : base(cloudEventBus, settings) { }

    public async Task Handle(DialogElementUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = domainEvent.DialogElementId.ToString()
        };

        if (domainEvent.RelatedDialogElementId is not null)
        {
            data["relatedDialogElementId"] = domainEvent.RelatedDialogElementId;
        }

        if (domainEvent.Type is not null)
        {
            data["dialogElementType"] = domainEvent.Type;
        }

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{DialogportenBaseUrl()}/api/v1/enduser/dialogs/{domainEvent.DialogId}/elements/{domainEvent.DialogElementId}",
            Data = data
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogElementCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = domainEvent.DialogElementId.ToString()
        };

        if (domainEvent.RelatedDialogElementId is not null)
        {
            data["relatedDialogElementId"] = domainEvent.RelatedDialogElementId;
        }

        if (domainEvent.Type is not null)
        {
            data["dialogElementType"] = domainEvent.Type;
        }

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{DialogportenBaseUrl()}/api/v1/enduser/dialogs/{domainEvent.DialogId}/elements/{domainEvent.DialogElementId}",
            Data = data
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogElementDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = domainEvent.DialogElementId.ToString()
        };

        if (domainEvent.RelatedDialogElementId is not null)
        {
            data["relatedDialogElementId"] = domainEvent.RelatedDialogElementId;
        }

        if (domainEvent.Type is not null)
        {
            data["dialogElementType"] = domainEvent.Type;
        }

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{DialogportenBaseUrl()}/api/v1/enduser/dialogs/{domainEvent.DialogId}/elements/{domainEvent.DialogElementId}",
            Data = data
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }
}
