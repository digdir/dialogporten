using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Events;

internal sealed class DomainEventToAltinnForwarder :
    INotificationHandler<DialogueCreatedDomainEvent>,
    INotificationHandler<DialogueUpdatedDomainEvent>,
    INotificationHandler<DialogueDeletedDomainEvent>,
    INotificationHandler<DialogueActivityCreatedDomainEvent>
{
    // TODO: Remove
    private const string SuperSimpleResource = "urn:altinn:resource:super-simple-service";

    private readonly ICloudEventBus _cloudEventBus;
    private readonly IDialogueDbContext _db;

    public DomainEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogueDbContext db)
    {
        _cloudEventBus = cloudEventBus ?? throw new ArgumentNullException(nameof(cloudEventBus));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(DialogueCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string DialogueCreated = "dialogporten.dialog.created.v1";
        Console.WriteLine($"Dialogue {notification.DialogueId} created.");

        var dialogue = await _db.Dialogues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogueId, cancellationToken);

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
            Resource = SuperSimpleResource, //dialogue.ServiceResourceIdentifier, 
            ResourceInstance = dialogue.Id.ToString(), 
            Subject = dialogue.Party, 
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogueId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogueUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string DialogueUpdated = "dialogporten.dialog.updated.v1";
        Console.WriteLine($"Dialogue {notification.DialogueId} updated.");
        var dialogue = await _db.Dialogues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogueId, cancellationToken);

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
            Resource = SuperSimpleResource, //dialogue.ServiceResourceIdentifier,
            ResourceInstance = dialogue.Id.ToString(),
            Subject = dialogue.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogueId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogueDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string DialogueDeleted = "dialogporten.dialog.deleted.v1";
        Console.WriteLine($"Dialogue {notification.DialogueId} deleted.");
        var dialogue = await _db.Dialogues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogueId, cancellationToken);

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
            Resource = SuperSimpleResource, //dialogue.ServiceResourceIdentifier,
            ResourceInstance = dialogue.Id.ToString(),
            Subject = dialogue.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogueId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    // TODO: Make dialogporten base url configurable

    public async Task Handle(DialogueActivityCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var dialogueActivity = await _db.DialogueActivities
            .Include(x => x.Dialogue)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogueActivityId, cancellationToken);

        if (dialogueActivity is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("DialogueActivity not found!");
        }

        var cloudEventType = dialogueActivity.Type.Id switch
        {
            DialogueActivityType.Enum.Submission => "dialogporten.dialog.activity.submission.v1",
            DialogueActivityType.Enum.Feedback => "dialogporten.dialog.activity.feedback.v1",
            DialogueActivityType.Enum.Information => "dialogporten.dialog.activity.information.v1",
            DialogueActivityType.Enum.Error => "dialogporten.dialog.activity.error.v1",
            DialogueActivityType.Enum.Closed => "dialogporten.dialog.activity.closed.v1",
            DialogueActivityType.Enum.Seen => "dialogporten.dialog.activity.seen.v1",
            DialogueActivityType.Enum.Forwarded => "dialogporten.dialog.activity.forwarded.v1",
            _ => throw new ArgumentOutOfRangeException()
        };

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = cloudEventType,
            Time = notification.OccuredAtUtc,
            Resource = SuperSimpleResource, //dialogueActivity.Dialogue.ServiceResourceIdentifier,
            ResourceInstance = dialogueActivity.Dialogue.Id.ToString(),
            Subject = dialogueActivity.Dialogue.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogueActivity.Dialogue.Id}/activityhistory/{dialogueActivity.Id}",
            Data =
            {
                // TODO: Add stuff
                ["activityId"] = dialogueActivity.Id,
                //["relatedActivityId"] = dialogueActivity.RelatedActivityId,
                ["extendedActivityType"] = dialogueActivity.ExtendedType,
                //["dialogElementId"] = dialogueActivity.DialogueElementId
            }
        };

        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }
}
