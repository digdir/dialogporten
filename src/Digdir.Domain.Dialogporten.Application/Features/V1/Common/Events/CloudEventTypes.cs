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
        DialogActivityType.Values.Submission => "dialogporten.dialog.activity.submission.v1",
        DialogActivityType.Values.Feedback => "dialogporten.dialog.activity.feedback.v1",
        DialogActivityType.Values.Information => "dialogporten.dialog.activity.information.v1",
        DialogActivityType.Values.Error => "dialogporten.dialog.activity.error.v1",
        DialogActivityType.Values.Closed => "dialogporten.dialog.activity.closed.v1",
        DialogActivityType.Values.Seen => "dialogporten.dialog.activity.seen.v1",
        DialogActivityType.Values.Forwarded => "dialogporten.dialog.activity.forwarded.v1",

        _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
    };
}
