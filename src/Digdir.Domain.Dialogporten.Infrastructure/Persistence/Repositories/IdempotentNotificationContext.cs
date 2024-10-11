using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Repositories;

internal sealed class IdempotentNotificationContext : IIdempotentNotificationContext
{
    private readonly HashSet<NotificationAcknowledgementPart> _acknowledgedParts = [];
    public Task AcknowledgePart(NotificationAcknowledgementPart acknowledgementPart,
        CancellationToken cancellationToken = default)
    {
        _acknowledgedParts.Add(acknowledgementPart);
        return Task.CompletedTask;
    }

    public Task<bool> IsAcknowledged(NotificationAcknowledgementPart acknowledgementPart, CancellationToken cancellationToken = default)
        => Task.FromResult(_acknowledgedParts.Contains(acknowledgementPart));

    public Task AcknowledgeWhole(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var parts = _acknowledgedParts.RemoveWhere(x => x.EventId == domainEvent.EventId);
        Console.WriteLine($"Notification acknowledged with event id {domainEvent.EventId} - {parts} acknowledgements removed.");
        return Task.CompletedTask;
    }

    public Task Load(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Loading notification with event id {domainEvent.EventId}");
        return Task.CompletedTask;
    }
}
