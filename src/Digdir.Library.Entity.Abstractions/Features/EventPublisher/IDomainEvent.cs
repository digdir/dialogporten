using MediatR;

namespace Digdir.Library.Entity.Abstractions.Features.EventPublisher;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccuredAt { get; set; } 
}