using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

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

        // Dialog activity
        nameof(DialogActivityType.Values.DialogCreated) => "dialogporten.dialog.activity.created.v1",
        nameof(DialogActivityType.Values.DialogCompleted) => "dialogporten.dialog.activity.completed.v1",
        nameof(DialogActivityType.Values.DialogClosed) => "dialogporten.dialog.activity.closed.v1",
        nameof(DialogActivityType.Values.Information) => "dialogporten.dialog.activity.information.v1",
        nameof(DialogActivityType.Values.TransmissionOpened) => "dialogporten.dialog.activity.transmission-opened.v1",
        nameof(DialogActivityType.Values.PaymentMade) => "dialogporten.dialog.activity.payment-made.v1",
        nameof(DialogActivityType.Values.SignatureProvided) => "dialogporten.dialog.activity.signature-provided.v1",

        _ => throw new ArgumentOutOfRangeException(nameof(eventName), eventName, null)
    };
}
