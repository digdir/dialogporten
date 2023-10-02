using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal static class CloudEventTypes
{
    internal static string Get(object obj) => obj switch
    {
        // Dialog 
        DialogCreatedDomainEvent => "dialogporten.dialog.created.v1",
        DialogUpdatedDomainEvent => "dialogporten.dialog.updated.v1",
        DialogDeletedDomainEvent => "dialogporten.dialog.deleted.v1",
        DialogReadDomainEvent => "dialogporten.dialog.read.v1",
        
        // DialogElement
        DialogElementDeletedDomainEvent => "dialogporten.dialog.element.deleted.v1",
        DialogElementCreatedDomainEvent => "dialogporten.dialog.element.created.v1",
        DialogElementUpdatedDomainEvent => "dialogporten.dialog.element.updated.v1", 
        
        // Dialog activity
        DialogActivityType.Enum.Submission => "dialogporten.dialog.activity.submission.v1",
        DialogActivityType.Enum.Feedback => "dialogporten.dialog.activity.feedback.v1",
        DialogActivityType.Enum.Information => "dialogporten.dialog.activity.information.v1",
        DialogActivityType.Enum.Error => "dialogporten.dialog.activity.error.v1",
        DialogActivityType.Enum.Closed => "dialogporten.dialog.activity.closed.v1",
        DialogActivityType.Enum.Seen => "dialogporten.dialog.activity.seen.v1",
        DialogActivityType.Enum.Forwarded => "dialogporten.dialog.activity.forwarded.v1", 
        
        _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
    };
}