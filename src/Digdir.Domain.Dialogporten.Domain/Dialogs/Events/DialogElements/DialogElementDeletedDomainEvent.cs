using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;

public sealed record DialogElementDeletedDomainEvent(
    Guid DialogId,
    Guid DialogElementId,
    Guid? RelatedDialogElementId,
    Uri? Type) :
    DomainEvent;