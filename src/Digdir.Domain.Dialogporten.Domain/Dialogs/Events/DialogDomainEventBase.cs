using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

public record DialogDomainEventBase(
    string? Process,
    string? PrecedingProcess) : DomainEvent;
