using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogElementEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogElementUpdatedDomainEvent>,
    INotificationHandler<DialogElementCreatedDomainEvent>,
    INotificationHandler<DialogElementDeletedDomainEvent>
{
    public DialogElementEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogDbContext db, IConfiguration configuration)
        : base(cloudEventBus, db, configuration)
    {
    }

    public async Task Handle(DialogElementUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialogElement = await GetDialogElement(domainEvent.DialogElementId, cancellationToken);
        var cloudEvent = CreateCloudEvent(dialogElement, domainEvent);
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogElementCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialogElement = await GetDialogElement(domainEvent.DialogElementId, cancellationToken);
        var cloudEvent = CreateCloudEvent(dialogElement, domainEvent);
        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogElementDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialog = await GetDialog(domainEvent.DialogId, cancellationToken);

        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = domainEvent.DialogElementId.ToString()
        };

        if (domainEvent.RelatedDialogElementId is not null)
        {
            data["relatedDialogElementId"] = domainEvent.RelatedDialogElementId.Value.ToString();
        }

        if (domainEvent.Type is not null)
        {
            data["dialogElementType"] = domainEvent.Type.ToString();
        }

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(domainEvent),
            Time = domainEvent.OccuredAt,
            Resource = dialog.ServiceResource,
            ResourceInstance = dialog.Id.ToString(),
            Subject = dialog.Party,
            Source = $"{DialogportenBaseUrl()}/api/v1/enduser/dialogs/{dialog.Id}/elements/{domainEvent.DialogElementId}",
            Data = data
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    private CloudEvent CreateCloudEvent(DialogElement dialogElement, IDomainEvent domainEvent) => new()
    {
        Id = domainEvent.EventId,
        Type = CloudEventTypes.Get(domainEvent),
        Time = domainEvent.OccuredAt,
        Resource = dialogElement.Dialog.ServiceResource,
        ResourceInstance = dialogElement.Dialog.Id.ToString(),
        Subject = dialogElement.Dialog.Party,
        Source = $"{DialogportenBaseUrl()}/api/v1/enduser/dialogs/{dialogElement.Dialog.Id}/elements/{dialogElement.Id}",
        Data = GetCloudEventData(dialogElement)
    };

    private static Dictionary<string, object> GetCloudEventData(DialogElement dialogElement)
    {
        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = dialogElement.Id.ToString()
        };

        if (dialogElement.RelatedDialogElement is not null)
        {
            data["relatedDialogElementId"] = dialogElement.RelatedDialogElement.Id.ToString();
        }

        if (dialogElement.Type is not null)
        {
            data["dialogElementType"] = dialogElement.Type.ToString();
        }

        return data;
    }

    private async Task<DialogElement> GetDialogElement(Guid dialogElementId, CancellationToken cancellationToken)
    {
        var dialogElement = await Db.DialogElements
            .Include(e => e.Dialog)
            .Include(e => e.RelatedDialogElement)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dialogElementId, cancellationToken)
            ?? throw new KeyNotFoundException($"DialogElement with id {dialogElementId} not found");

        return dialogElement;
    }
}
