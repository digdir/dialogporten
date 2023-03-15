using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public sealed record DialogueCreatedDomainEvent(Guid DialogueId) : DomainEvent;
