using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

public record DialogActivityCreatedDomainEvent(Guid DialogId, Guid DialogActivityId) : DomainEvent;
