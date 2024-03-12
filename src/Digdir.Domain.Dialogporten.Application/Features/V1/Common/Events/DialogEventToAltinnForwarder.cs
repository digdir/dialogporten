using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MediatR;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogCreatedDomainEvent>,
    INotificationHandler<DialogUpdatedDomainEvent>,
    INotificationHandler<DialogDeletedDomainEvent>,
    INotificationHandler<DialogSeenDomainEvent>
{
    public DialogEventToAltinnForwarder(ICloudEventBus cloudEventBus, IOptions<ApplicationSettings> settings)
        : base(cloudEventBus, settings) { }

    public async Task Handle(DialogCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(nameof(DialogCreatedDomainEvent)),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{SourceBaseUrl()}{domainEvent.DialogId}"
        };
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(nameof(DialogUpdatedDomainEvent)),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{SourceBaseUrl()}{domainEvent.DialogId}"
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogSeenDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(nameof(DialogSeenDomainEvent)),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{SourceBaseUrl()}{domainEvent.DialogId}"
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(nameof(DialogDeletedDomainEvent)),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{SourceBaseUrl()}{domainEvent.DialogId}"
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }
}
