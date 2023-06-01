using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Events;

internal sealed class DomainEventToAltinnForwarder :
    INotificationHandler<DialogueCreatedDomainEvent>,
    INotificationHandler<DialogueUpdatedDomainEvent>,
    INotificationHandler<DialogueDeletedDomainEvent>
{
    private const string DialogueCreated = "dialogporten.dialog.created.v1";
    private const string DialogueUpdated = "dialogporten.dialog.updated.v1";
    private const string DialogueDeleted = "dialogporten.dialog.deleted.v1";

    private readonly ICloudEventBus _cloudEventBus;
    private readonly IDialogueDbContext _db;

    public DomainEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogueDbContext db)
    {
        _cloudEventBus = cloudEventBus ?? throw new ArgumentNullException(nameof(cloudEventBus));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(DialogueCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} created.");

        var dialogue = await _db.Dialogues.FirstOrDefaultAsync(x => x.Id == notification.DialogueId, cancellationToken);

        if (dialogue is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("Dialogue not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = DialogueCreated,
            Time = notification.OccuredAtUtc,
            Resource = dialogue.ServiceResourceIdentifier, 
            ResourceInstance = dialogue.Id.ToString(), 
            Subject = dialogue.Party, 
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogueId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogueUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} updated.");
        var dialogue = await _db.Dialogues.FirstOrDefaultAsync(x => x.Id == notification.DialogueId, cancellationToken);

        if (dialogue is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("Dialogue not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = DialogueUpdated,
            Time = notification.OccuredAtUtc,
            Resource = dialogue.ServiceResourceIdentifier,
            ResourceInstance = dialogue.Id.ToString(),
            Subject = dialogue.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogueId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogueDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} deleted.");
        var dialogue = await _db.Dialogues.FirstOrDefaultAsync(x => x.Id == notification.DialogueId, cancellationToken);

        if (dialogue is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("Dialogue not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = DialogueDeleted,
            Time = notification.OccuredAtUtc,
            Resource = dialogue.ServiceResourceIdentifier,
            ResourceInstance = dialogue.Id.ToString(),
            Subject = dialogue.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogueId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }
}
