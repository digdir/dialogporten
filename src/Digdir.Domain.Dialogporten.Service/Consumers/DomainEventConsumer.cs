using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
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
        try
        {
            await _notificationContext.LoadAcknowledgements(context.Message, context.CancellationToken);
            await _publisher.Publish(context.Message, context.CancellationToken);
            await _notificationContext.AcknowledgeWhole(context.Message, context.CancellationToken);
        }
        catch (Exception)
        {
            await _notificationContext.NotAcknowledgeWhole(context.CancellationToken);
            throw;
        }
    }
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
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30)));
        endpointConfigurator.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(800),
            TimeSpan.FromMilliseconds(1000)));
    }
}
