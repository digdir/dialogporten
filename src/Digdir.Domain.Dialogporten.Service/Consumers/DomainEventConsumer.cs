using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.IdempotentNotifications;
using MassTransit;
using MediatR;

namespace Digdir.Domain.Dialogporten.Service.Consumers;

public sealed class DomainEventConsumer<T> : IConsumer<T>
    where T : class, IDomainEvent
{
    private readonly IPublisher _publisher;
    private readonly IIdempotentNotificationContext _notificationContext;

    public DomainEventConsumer(IPublisher publisher, IIdempotentNotificationContext notificationContext)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _notificationContext = notificationContext ?? throw new ArgumentNullException(nameof(notificationContext));
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        var isFirstAttempt = IsFirstAttempt(context);
        await using var transaction = await _notificationContext
            .BeginTransaction(context.Message, isFirstAttempt, context.CancellationToken);
        await _publisher.Publish(context.Message, context.CancellationToken);
        await transaction.Ack(context.CancellationToken);
    }

    private static bool IsFirstAttempt(ConsumeContext<T> context)
        => (context.GetRetryAttempt() + context.GetRedeliveryCount()) == 0;
}

public sealed class DomainEventConsumerDefinition<T> : ConsumerDefinition<DomainEventConsumer<T>>
    where T : class, IDomainEvent
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DomainEventConsumer<T>> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseDelayedRedelivery(r => r.Intervals(
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(15)));
        endpointConfigurator.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(800),
            TimeSpan.FromMilliseconds(1000)));
    }
}
