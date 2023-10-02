using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;

public sealed record DialogElementUpdatedDomainEvent(Guid DialogId, Guid DialogElementId) : DomainEvent;