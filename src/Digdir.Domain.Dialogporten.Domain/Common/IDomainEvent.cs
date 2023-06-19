using MediatR;
using System.Text.Json.Serialization;

namespace Digdir.Domain.Dialogporten.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccuredAt { get; set; } 
}

public abstract record DomainEvent : IDomainEvent
{
    [JsonInclude]
    public Guid EventId { get; private set; } = Guid.NewGuid();

    [JsonInclude]
    public DateTimeOffset OccuredAt { get; set; }
}
