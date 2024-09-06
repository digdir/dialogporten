using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Events;

public sealed record DialogCreatedDomainEvent(Guid DialogId, string ServiceResource, string Party, string? Process, string? PrecedingProcess) : DialogDomainEventBase(Process, PrecedingProcess);
