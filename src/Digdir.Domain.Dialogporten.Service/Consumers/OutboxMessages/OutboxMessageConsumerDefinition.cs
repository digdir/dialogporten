using MassTransit;

namespace Digdir.Domain.Dialogporten.Service.Consumers.OutboxMessages;

public sealed class OutboxMessageConsumerDefinition : ConsumerDefinition<OutboxMessageConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<OutboxMessageConsumer> consumerConfigurator,
        IRegistrationContext context) => endpointConfigurator.ConfigureConsumeTopology = false;
}
