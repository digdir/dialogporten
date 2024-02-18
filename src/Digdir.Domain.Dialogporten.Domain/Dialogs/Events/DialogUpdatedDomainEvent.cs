using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

public sealed record DialogUpdatedDomainEvent(Guid DialogId, string ServiceResource, string Party) : DomainEvent;
