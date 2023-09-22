using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DomainEventToAltinnForwarder :
    INotificationHandler<DialogCreatedDomainEvent>,
    INotificationHandler<DialogUpdatedDomainEvent>,
    INotificationHandler<DialogDeletedDomainEvent>,
    INotificationHandler<DialogReadDomainEvent>,
    INotificationHandler<DialogActivityCreatedDomainEvent>,
    INotificationHandler<DialogElementUpdatedDomainEvent>,
    INotificationHandler<DialogElementCreatedDomainEvent>,
    INotificationHandler<DialogElementDeletedDomainEvent>
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
            throw new ApplicationException("Dialog not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogCreated,
            Time = notification.OccuredAt,
            Resource = dialog.ServiceResource.ToString(),
            ResourceInstance = dialog.Id.ToString(), 
            Subject = dialog.Party, 
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
            throw new ApplicationException("Dialog not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogUpdated,
            Time = notification.OccuredAt,
            Resource = dialog.ServiceResource.ToString(),
            ResourceInstance = dialog.Id.ToString(),
            Subject = dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogId}",
            Data = notification.ModifiedPaths.Count == 0 ? null : new()
            {
                ["modifiedPaths"] = notification.ModifiedPaths
            }
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogDeletedDomainEvent @event, CancellationToken cancellationToken)
    {
        const string dialogDeleted = "dialogporten.dialog.deleted.v1";
        Console.WriteLine($"Dialog {@event.DialogId} deleted.");
        // Cannot retrieve dialog from db since it is deleted
        var cloudEvent = new CloudEvent
        {
            Id = @event.EventId,
            Type = dialogDeleted,
            Time = @event.OccuredAt,
            Resource = @event.ServiceResource,
            ResourceInstance = @event.DialogId.ToString(),
            Subject = @event.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{@event.DialogId}"
        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    // TODO: Make dialogporten base url configurable

    public async Task Handle(DialogActivityCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var dialogActivity = await _db.DialogActivities
            .Include(e => e.Dialog)
            .Include(e => e.DialogElement)
            .Include(e => e.RelatedActivity)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogActivityId, cancellationToken);

        if (dialogActivity is null)
        {
            // TODO: Improve exception or handle differently
            throw new ApplicationException("DialogActivity not found!");
        }

        var cloudEventType = dialogActivity.TypeId switch
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
            Time = notification.OccuredAt,
            Resource = dialogActivity.Dialog.ServiceResource.ToString(),
            ResourceInstance = dialogActivity.Dialog.Id.ToString(),
            Subject = dialogActivity.Dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogActivity.Dialog.Id}/activities/{dialogActivity.Id}",
            Data = GetCloudEventData(dialogActivity)
        };

        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogReadDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogRead = "dialogporten.dialog.read.v1";
        Console.WriteLine($"Dialog {notification.DialogId} read.");
        var dialog = await _db.Dialogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogId, cancellationToken);

        if (dialog is null)
        {
            // TODO: Improve exception or handle differently
            throw new ApplicationException("Dialog not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogRead,
            Time = notification.OccuredAt,
            Resource = dialog.ServiceResource.ToString(),
            ResourceInstance = dialog.Id.ToString(),
            Subject = dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{notification.DialogId}",

        };
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    private Dictionary<string, object> GetCloudEventData(DialogActivity dialogActivity)
    {
        var data = new Dictionary<string, object>
        {
            ["activityId"] = dialogActivity.Id.ToString(),
        };

        if (dialogActivity.ExtendedType is not null)
        {
            data["extendedActivityType"] = dialogActivity.ExtendedType.ToString();
        }

        if (dialogActivity.RelatedActivity is not null)
        {
            data["extendedActivityType"] = dialogActivity.RelatedActivity.Id.ToString()!;
        }

        if (dialogActivity.DialogElement is null) return data;

        data["dialogElementId"] = dialogActivity.DialogElement.Id.ToString();
        if (dialogActivity.DialogElement.Type is not null)
        {
            data["dialogElementType"] = dialogActivity.DialogElement.Type.ToString();
        }

        if (dialogActivity.DialogElement.RelatedDialogElement is not null)
        {
            data["relatedDialogElementId"] = dialogActivity.DialogElement.RelatedDialogElement.Id.ToString()!;
        }

        return data;
    }

    public async Task Handle(DialogElementUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogElementUpdated = "dialogporten.dialog.element.updated.v1";
        
        var dialogElement = await _db.DialogElements
            .Include(e => e.Dialog)
            .Include(e => e.RelatedDialogElement)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogElementId, cancellationToken);

        if (dialogElement is null)
        {
            // TODO: Improve exception or handle differently
            throw new ApplicationException("DialogElement not found!");
        }

        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = dialogElement.Id.ToString()
        };
        
        if (dialogElement.RelatedDialogElement is not null)
        {
            data["relatedDialogElementId"] = dialogElement.RelatedDialogElement.Id.ToString();
        }
        
        if(dialogElement.Type is not null)
        {
            data["dialogElementType"] = dialogElement.Type.ToString();
        }
        
        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogElementUpdated,
            Time = notification.OccuredAt,
            Resource = dialogElement.Dialog.ServiceResource.ToString(),
            ResourceInstance = dialogElement.Dialog.Id.ToString(),
            Subject = dialogElement.Dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogElement.Dialog.Id}/elements/{dialogElement.Id}",
            Data = data
        };
        
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogElementCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogElementCreated = "dialogporten.dialog.element.created.v1";
        
        var dialogElement = await _db.DialogElements
            .Include(e => e.Dialog)
            .Include(e => e.RelatedDialogElement)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogElementId, cancellationToken);

        if (dialogElement is null)
        {
            // TODO: Improve exception or handle differently
            throw new ApplicationException("DialogElement not found!");
        }
        
        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = dialogElement.Id.ToString()
        };
        
        if (dialogElement.RelatedDialogElement is not null)
        {
            data["relatedDialogElementId"] = dialogElement.RelatedDialogElement.Id.ToString();
        }
        
        if(dialogElement.Type is not null)
        {
            data["dialogElementType"] = dialogElement.Type.ToString();
        }
        
        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogElementCreated,
            Time = notification.OccuredAt,
            Resource = dialogElement.Dialog.ServiceResource.ToString(),
            ResourceInstance = dialogElement.Dialog.Id.ToString(),
            Subject = dialogElement.Dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogElement.Dialog.Id}/elements/{dialogElement.Id}",
            Data = data
        };
        
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }

    public async Task Handle(DialogElementDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        const string dialogElementDeleted = "dialogporten.dialog.element.deleted.v1";
        var dialogElement = await _db.DialogElements
            .Include(e => e.Dialog)
            .Include(e => e.RelatedDialogElement)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.DialogElementId, cancellationToken);

        if (dialogElement is null)
        {
            // TODO: Improve exception or handle differently
            throw new ApplicationException("DialogElement not found!");
        }
        
        var data = new Dictionary<string, object>
        {
            ["dialogElementId"] = dialogElement.Id.ToString()
        };
        
        if (dialogElement.RelatedDialogElement is not null)
        {
            data["relatedDialogElementId"] = dialogElement.RelatedDialogElement.Id.ToString();
        }
        
        if(dialogElement.Type is not null)
        {
            data["dialogElementType"] = dialogElement.Type.ToString();
        }
        
        var cloudEvent = new CloudEvent
        {
            Id = notification.EventId,
            Type = dialogElementDeleted,
            Time = notification.OccuredAt,
            Resource = dialogElement.Dialog.ServiceResource.ToString(),
            ResourceInstance = dialogElement.Dialog.Id.ToString(),
            Subject = dialogElement.Dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogElement.Dialog.Id}/elements/{dialogElement.Id}",
            Data = data
        };
        
        await _cloudEventBus.Publish(cloudEvent, cancellationToken);
    }
}
