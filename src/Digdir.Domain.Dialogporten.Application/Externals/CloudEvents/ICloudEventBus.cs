namespace Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;

public interface ICloudEventBus
{
    Task Publish(CloudEvent cloudEvent, CancellationToken cancellationToken);
}
