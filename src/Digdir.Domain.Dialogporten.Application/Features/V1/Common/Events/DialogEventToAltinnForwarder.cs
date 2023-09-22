using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogCreatedDomainEvent>,
    INotificationHandler<DialogUpdatedDomainEvent>,
    INotificationHandler<DialogDeletedDomainEvent>,
    INotificationHandler<DialogReadDomainEvent>
{
    public DialogEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogDbContext db)
        : base(cloudEventBus, db) { }
    
    public async Task Handle(DialogCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = dialog.ServiceResource.ToString(),
            ResourceInstance = dialog.Id.ToString(), 
            Subject = dialog.Party, 
            Source = $"https://dialogporten.no/api/v1/dialogs/{domainEvent.DialogId}"
        };
        
        await CloudEventBus.Publish(cloudEvent, cancellationToken); 
        // Console.WriteLine($"Dialog {notification.DialogId} created.");
    }

    public async Task Handle(DialogUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = dialog.ServiceResource.ToString(),
            ResourceInstance = dialog.Id.ToString(),
            Subject = dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{domainEvent.DialogId}",
            Data = domainEvent.ModifiedPaths.Count == 0 ? null : new()
            {
                ["modifiedPaths"] = domainEvent.ModifiedPaths
            }
        };
        
        await CloudEventBus.Publish(cloudEvent, cancellationToken); 
        // Console.WriteLine($"Dialog {notification.DialogId} updated.");
    }

    public async Task Handle(DialogReadDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = dialog.ServiceResource.ToString(),
            ResourceInstance = dialog.Id.ToString(),
            Subject = dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{domainEvent.DialogId}",
        };
        
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
        // Console.WriteLine($"Dialog {notification.DialogId} deleted.");
    }
    
    public async Task Handle(DialogDeletedDomainEvent @event, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = @event.EventId,
            Type = CloudEventTypes.Get(@event),
            Time = @event.OccuredAt,
            Resource = @event.ServiceResource,
            ResourceInstance = @event.DialogId.ToString(),
            Subject = @event.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{@event.DialogId}"
        };
        
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
        // Console.WriteLine($"Dialog {@event.DialogId} deleted.");
    }
}