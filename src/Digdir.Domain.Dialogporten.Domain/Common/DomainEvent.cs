using System.Text.Json.Serialization;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;

namespace Digdir.Domain.Dialogporten.Domain.Common;



public abstract record DomainEvent : IDomainEvent
{
    [JsonInclude]
    public Guid EventId { get; private set; } = Guid.NewGuid();

    [JsonInclude]
    public DateTimeOffset OccuredAt { get; set; }
}
