using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal sealed class DialogActivityEventToAltinnForwarder : DomainEventToAltinnForwarderBase,
    INotificationHandler<DialogActivityCreatedDomainEvent>
{
    internal DialogActivityEventToAltinnForwarder(ICloudEventBus cloudEventBus, IDialogDbContext db) 
        : base(cloudEventBus, db) { }

    public async Task Handle(DialogActivityCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dialogActivity = await Db.DialogActivities
            .Include(e => e.Dialog)
            .Include(e => e.DialogElement)
            .Include(e => e.RelatedActivity)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == domainEvent.DialogActivityId, cancellationToken);

        if (dialogActivity is null)
        {
            // TODO: Improve exception or handle differently
            throw new ApplicationException("DialogActivity not found!");
        }

        var cloudEvent = new CloudEvent
        {
            Id = domainEvent.EventId,
            Type = CloudEventTypes.Get(dialogActivity.TypeId),
            Time = domainEvent.OccuredAt,
            Resource = dialogActivity.Dialog.ServiceResource.ToString(),
            ResourceInstance = dialogActivity.Dialog.Id.ToString(),
            Subject = dialogActivity.Dialog.Party,
            Source = $"https://dialogporten.no/api/v1/dialogs/{dialogActivity.Dialog.Id}/activities/{dialogActivity.Id}",
            Data = GetCloudEventData(dialogActivity)
        };

        await CloudEventBus.Publish(cloudEvent, cancellationToken); 
    }
    
    private static Dictionary<string, object> GetCloudEventData(DialogActivity dialogActivity)
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
}