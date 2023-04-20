using MediatR;
using System.Text.Json.Serialization;

namespace Digdir.Domain.Dialogporten.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    [JsonInclude]
    public Guid EventId { get; private set; } = Guid.NewGuid();
}
