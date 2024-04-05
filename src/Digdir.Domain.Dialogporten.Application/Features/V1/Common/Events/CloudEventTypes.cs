using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events;

internal static class CloudEventTypes
{
    internal static string Get(string eventName) => eventName switch
    {
        // Dialog
        nameof(DialogCreatedDomainEvent) => "dialogporten.dialog.created.v1",
        nameof(DialogUpdatedDomainEvent) => "dialogporten.dialog.updated.v1",
        nameof(DialogDeletedDomainEvent) => "dialogporten.dialog.deleted.v1",
        nameof(DialogSeenDomainEvent) => "dialogporten.dialog.seen.v1",

        // DialogElement
        nameof(DialogElementDeletedDomainEvent) => "dialogporten.dialog.element.deleted.v1",
        nameof(DialogElementCreatedDomainEvent) => "dialogporten.dialog.element.created.v1",
        nameof(DialogElementUpdatedDomainEvent) => "dialogporten.dialog.element.updated.v1",

        // Dialog activity
        nameof(DialogActivityType.Values.Submission) => "dialogporten.dialog.activity.submission.v1",
        nameof(DialogActivityType.Values.Feedback) => "dialogporten.dialog.activity.feedback.v1",
        nameof(DialogActivityType.Values.Information) => "dialogporten.dialog.activity.information.v1",
        nameof(DialogActivityType.Values.Error) => "dialogporten.dialog.activity.error.v1",
        nameof(DialogActivityType.Values.Closed) => "dialogporten.dialog.activity.closed.v1",
        nameof(DialogActivityType.Values.Forwarded) => "dialogporten.dialog.activity.forwarded.v1",

        _ => throw new ArgumentOutOfRangeException(nameof(eventName), eventName, null)
    };
}
