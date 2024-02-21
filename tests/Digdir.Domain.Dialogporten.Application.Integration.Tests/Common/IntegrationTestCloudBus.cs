using Digdir.Domain.Dialogporten.Application.Externals;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

internal sealed class IntegrationTestCloudBus : ICloudEventBus
{
    public List<CloudEvent> Events { get; } = [];
    public Task Publish(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        Events.Add(cloudEvent);
        return Task.CompletedTask;
    }
}
