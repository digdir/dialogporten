using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Events;

public record DialogueActivityCreatedDomainEvent(Guid DialogueId, Guid DialogueActivityId) : DomainEvent;