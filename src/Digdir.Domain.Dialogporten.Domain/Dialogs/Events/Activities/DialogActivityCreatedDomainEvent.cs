using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;

public sealed record DialogActivityCreatedDomainEvent(Guid DialogId, Guid DialogActivityId) : DomainEvent;
