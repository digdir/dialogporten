using System.Reflection;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using Digdir.Domain.Dialogporten.Service.Consumers;
using MediatR;
using EndpointNameAttribute = Digdir.Domain.Dialogporten.Application.Common.EndpointNameAttribute;

namespace Digdir.Domain.Dialogporten.Service.Common;

internal static class Extensions
{
    public static ApplicationConsumerMapping[] GetApplicationConsumerMaps()
    {
        var openNotificationHandler = typeof(INotificationHandler<>);
        var openDomainEventConsumer = typeof(DomainEventConsumer<,>);
        var openDomainEventConsumerDefinition = typeof(DomainEventConsumerDefinition<,>);
        var domainEventType = typeof(IDomainEvent);
        return ApplicationAssemblyMarker.Assembly.DefinedTypes
            .Where(x => x is { IsInterface: false, IsAbstract: false })
            .SelectMany(x => x
                .GetInterfaces()
                .Where(@interface =>
                    @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == openNotificationHandler
                    && @interface.GetGenericArguments().Single().IsAssignableTo(domainEventType))
                .Select(@interface => (@class: x, @interface, @event: @interface.GetGenericArguments().Single()))
                .Select(x => new ApplicationConsumerMapping(
                    AppConsumerType: x.@class.AsType(),
                    BusConsumerType: openDomainEventConsumer.MakeGenericType(x.@class, x.@event),
                    BusDefinitionType: openDomainEventConsumerDefinition.MakeGenericType(x.@class, x.@event),
                    EndpointName: x.@class.GetInterfaceMap(x.@interface)
                        .TargetMethods.Single()
                        .GetCustomAttribute<EndpointNameAttribute>()?
                        .Name ?? $"{x.@class.Name}_{x.@event.Name}")
                ))
            .ToArray();
    }
}

internal record struct ApplicationConsumerMapping(Type AppConsumerType, Type BusConsumerType, Type BusDefinitionType, string EndpointName);
