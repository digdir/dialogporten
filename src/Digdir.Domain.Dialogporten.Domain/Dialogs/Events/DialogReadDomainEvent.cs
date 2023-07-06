using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

public sealed record DialogReadDomainEvent(Guid DialogId) : DomainEvent;