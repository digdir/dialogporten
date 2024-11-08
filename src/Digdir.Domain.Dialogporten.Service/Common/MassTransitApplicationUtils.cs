using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Service.Consumers;

namespace Digdir.Domain.Dialogporten.Service.Common;

internal static class MassTransitApplicationUtils
{
    public static ApplicationConsumerMapping[] GetApplicationConsumerMaps()
    {
        var openDomainEventConsumer = typeof(DomainEventConsumer<,>);
        var openDomainEventConsumerDefinition = typeof(DomainEventConsumerDefinition<,>);
        return ApplicationEventHandlerUtils
            .GetHandlerEventMaps()
            .Select(x => new ApplicationConsumerMapping(
                AppConsumerType: x.HandlerType,
                BusConsumerType: openDomainEventConsumer.MakeGenericType(x.HandlerType, x.EventType),
                BusDefinitionType: openDomainEventConsumerDefinition.MakeGenericType(x.HandlerType, x.EventType),
                EndpointName: x.EndpointName))
            .ToArray();
    }
}


internal record struct ApplicationConsumerMapping(Type AppConsumerType, Type BusConsumerType, Type BusDefinitionType, string EndpointName);
