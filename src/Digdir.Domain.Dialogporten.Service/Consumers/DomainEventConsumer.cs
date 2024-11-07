using System.Reflection;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using MassTransit;
using MediatR;
using EndpointNameAttribute = Digdir.Domain.Dialogporten.Application.Common.EndpointNameAttribute;

namespace Digdir.Domain.Dialogporten.Service.Consumers;

public sealed class DomainEventConsumer<THandler, TEvent>(THandler handler) : IConsumer<TEvent>
    where THandler : INotificationHandler<TEvent>
    where TEvent : class, IDomainEvent
{
    private readonly THandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    public Task Consume(ConsumeContext<TEvent> context) => _handler.Handle(context.Message, context.CancellationToken);
}

public sealed class DomainEventConsumerDefinition<THandler, TEvent> : ConsumerDefinition<DomainEventConsumer<THandler, TEvent>>
    where THandler : INotificationHandler<TEvent>
    where TEvent : class, IDomainEvent
{
    public DomainEventConsumerDefinition()
    {
        var @class = typeof(THandler);
        var @interface = typeof(INotificationHandler<TEvent>);
        var @event = typeof(TEvent);
        EndpointName = @class.GetInterfaceMap(@interface)
            .TargetMethods.Single()
            .GetCustomAttribute<EndpointNameAttribute>()?
            .Name ?? EndpointNameAttribute.DefaultName(@class, @event);
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DomainEventConsumer<THandler, TEvent>> consumerConfigurator,
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
