using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using MediatR;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogCreatedDomainEvent>,
    INotificationHandler<DialogUpdatedDomainEvent>,
    INotificationHandler<DialogDeletedDomainEvent>,
    INotificationHandler<DialogReadDomainEvent>
{
    public DialogEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogDbContext db,
        IOptions<ApplicationSettings> settings)
        : base(cloudEventBus, db, settings) { }

    public async Task Handle(DialogCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);
        var cloudEvent = CreateCloudEvent(domainEvent, dialog);
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);
        var cloudEvent = CreateCloudEvent(domainEvent, dialog);
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogReadDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);
        var cloudEvent = CreateCloudEvent(domainEvent, dialog);
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = domainEvent.ServiceResource,
            ResourceInstance = domainEvent.DialogId.ToString(),
            Subject = domainEvent.Party,
            Source = $"{DialogportenBaseUrl()}/api/v1/serviceowner/dialogs/{domainEvent.DialogId}"
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }
    
    private CloudEvent CreateCloudEvent(IDomainEvent domainEvent, DialogEntity dialog,
        Dictionary<string, object>? data = null) => new()
    {
        Id = domainEvent.EventId,
        Type = CloudEventTypes.Get(domainEvent),
        Time = domainEvent.OccuredAt,
        Resource = dialog.ServiceResource.ToString(),
        ResourceInstance = dialog.Id.ToString(),
        Subject = dialog.Party,
        Source = $"{DialogportenBaseUrl()}/api/v1/serviceowner/dialogs/{dialog.Id}",
        Data = data
    };
}