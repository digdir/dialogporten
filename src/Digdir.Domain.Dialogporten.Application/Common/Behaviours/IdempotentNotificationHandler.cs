using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Common.Behaviours;

public interface IIdempotentNotificationContext
{
    Task AcknowledgePart(NotificationAcknowledgementPart acknowledgementPart, CancellationToken cancellationToken = default);
    Task<bool> IsAcknowledged(NotificationAcknowledgementPart acknowledgementPart, CancellationToken cancellationToken = default);
    Task AcknowledgeWhole(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task Load(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public readonly record struct NotificationAcknowledgementPart(string ConsumerType, Guid EventId);

internal sealed class IdempotentNotificationHandler<TNotification>(
    INotificationHandler<TNotification> decorated,
    IIdempotentNotificationContext repository) :
    INotificationHandler<TNotification>,
    // We need to manually register this NotificationHandler because
    // it should decorate all INotificationHandler<TNotification>
    // instances, not be a notification handler in of itself.
    IIgnoreOnAssemblyScan
    where TNotification : IDomainEvent
{
    private readonly INotificationHandler<TNotification> _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    private readonly IIdempotentNotificationContext _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        var consumer = _decorated.GetType();
        var acknowledgement = new NotificationAcknowledgementPart(consumer.FullName!, notification.EventId);
        if (await _repository.IsAcknowledged(acknowledgement, cancellationToken))
        {
            // I've handled this event before, so I'm not going to do it again.
            return;
        }

        await _decorated.Handle(notification, cancellationToken);
        await _repository.AcknowledgePart(acknowledgement, cancellationToken);
    }
}
