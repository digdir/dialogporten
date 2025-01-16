using System.Reflection;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.Common.Events;

public class DomainEventTests
{
    [Fact]
    public void All_Altinn_Forwarders_Must_Check_For_Event_Opt_Out()
    {
        // Arrange
        var handlers = Assembly.GetAssembly(typeof(Digdir.Domain.Dialogporten.Application.Features.V1.Common.Events.AltinnForwarders.DomainEventToAltinnForwarderBase))
            ?.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
            .ToList();

        // Act & Assert
        foreach (var handler in handlers!)
        {
            var handleMethods = handler.GetMethods().Where(m => m.Name == "Handle" && m.GetParameters().Any(p => typeof(IDomainEvent).IsAssignableFrom(p.ParameterType)));
            foreach (var method in handleMethods)
            {
                var parameters = method.GetParameters();
                var domainEventType = parameters.First().ParameterType;
                var domainEventInstance = Activator.CreateInstance(domainEventType);
                var metadataProperty = domainEventType.GetProperty("Metadata");
                metadataProperty?.SetValue(domainEventInstance, new Dictionary<string, string> { { Constants.DisableAltinnEvents, bool.TrueString } });

                var handlerInstance = Activator.CreateInstance(handler);
                var cancellationToken = CancellationToken.None;

                var shouldNotBeSentToAltinnMethod = domainEventType.GetMethod("ShouldNotBeSentToAltinn");
                var shouldNotBeSentToAltinnResult = (bool)shouldNotBeSentToAltinnMethod?.Invoke(domainEventInstance, null)!;

                Assert.True(shouldNotBeSentToAltinnResult, $"The method {method.Name} in {handler.Name} does not check ShouldNotBeSentToAltinn.");
            }
        }
    }
}
