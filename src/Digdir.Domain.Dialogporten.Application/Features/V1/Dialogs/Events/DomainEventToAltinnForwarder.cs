using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Events;

internal sealed class DomainEventToAltinnForwarder :
    INotificationHandler<DialogCreatedDomainEvent>,
    INotificationHandler<DialogUpdatedDomainEvent>,
    INotificationHandler<DialogDeletedDomainEvent>,
    INotificationHandler<DialogActivityCreatedDomainEvent>
{

    private readonly ICloudEventBus _cloudEventBus;
    private readonly IDialogDbContext _db;

    public DomainEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogDbContext db)
    {
        _cloudEventBus = cloudEventBus ?? throw new ArgumentNullException(nameof(cloudEventBus));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(DialogCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogCreated = "dialogporten.dialog.created.v1";
        Console.WriteLine($"Dialog {notification.DialogId} created.");

        var dialog = await _db.Dialogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogId, cancellationToken);

        if (dialog is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("Dialog not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogCreated,
            Time = notification.OccuredAtUtc,
            Resource = dialog.ServiceResourceIdentifier, 
            ResourceInstance = dialog.Id.ToString(), 
            AlternativeSubject = dialog.Party, 
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogUpdated = "dialogporten.dialog.updated.v1";
        Console.WriteLine($"Dialog {notification.DialogId} updated.");
        var dialog = await _db.Dialogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogId, cancellationToken);

        if (dialog is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("Dialog not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogUpdated,
            Time = notification.OccuredAtUtc,
            Resource = dialog.ServiceResourceIdentifier,
            ResourceInstance = dialog.Id.ToString(),
            AlternativeSubject = dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogDeleted = "dialogporten.dialog.deleted.v1";
        Console.WriteLine($"Dialog {notification.DialogId} deleted.");
        var dialog = await _db.Dialogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogId, cancellationToken);

        if (dialog is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("Dialog not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogDeleted,
            Time = notification.OccuredAtUtc,
            Resource = dialog.ServiceResourceIdentifier,
            ResourceInstance = dialog.Id.ToString(),
            AlternativeSubject = dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    // TODO: Make dialogporten base url configurable

    public async Task Handle(DialogActivityCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var dialogActivity = await _db.DialogActivities
            .Include(x => x.Dialog)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogActivityId, cancellationToken);

        if (dialogActivity is null)
        {
            // TODO: Improve exception or handle differently
            throw new Exception("DialogActivity not found!");
        }

        var cloudEventType = dialogActivity.Type.Id switch
        {
            DialogActivityType.Enum.Submission => "dialogporten.dialog.activity.submission.v1",
            DialogActivityType.Enum.Feedback => "dialogporten.dialog.activity.feedback.v1",
            DialogActivityType.Enum.Information => "dialogporten.dialog.activity.information.v1",
            DialogActivityType.Enum.Error => "dialogporten.dialog.activity.error.v1",
            DialogActivityType.Enum.Closed => "dialogporten.dialog.activity.closed.v1",
            DialogActivityType.Enum.Seen => "dialogporten.dialog.activity.seen.v1",
            DialogActivityType.Enum.Forwarded => "dialogporten.dialog.activity.forwarded.v1",
            _ => throw new ArgumentOutOfRangeException()
        };

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = cloudEventType,
            Time = notification.OccuredAtUtc,
            Resource = dialogActivity.Dialog.ServiceResourceIdentifier,
            ResourceInstance = dialogActivity.Dialog.Id.ToString(),
            AlternativeSubject = dialogActivity.Dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogActivity.Dialog.Id}/activityhistory/{dialogActivity.Id}",
            Data = new Dictionary<string, object>
            {
                // TODO: Add stuff
                ["activityId"] = dialogActivity.Id,
                //["relatedActivityId"] = dialogActivity.RelatedActivityId,
                ["extendedActivityType"] = dialogActivity.ExtendedType,
                //["dialogElementId"] = dialogActivity.DialogElementId
            }
        };

        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }
}
