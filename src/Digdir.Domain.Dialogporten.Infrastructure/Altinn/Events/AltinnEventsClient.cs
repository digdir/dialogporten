using System.Net.Http.Headers;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Serialization;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Events;

internal class AltinnEventsClient : ICloudEventBus
{
    private readonly HttpClient _client;

    public AltinnEventsClient(HttpClient client)
    {
        _client = client;
    }

    public async Task Publish(CloudEvent cloudEvent, CancellationToken cancellationToken)
        => await _client.PostAsJsonEnsuredAsync(
            "/events/api/v1/events",
            cloudEvent,
            serializerOptions: SerializerOptions.CloudEventSerializerOptions,
            configureContentHeaders: h
                => h.ContentType = new MediaTypeHeaderValue("application/cloudevents+json"),
            cancellationToken: cancellationToken);
}

internal class ConsoleLogEventBus : ICloudEventBus
{
    private readonly ILogger<ConsoleLogEventBus> _logger;

    public ConsoleLogEventBus(ILogger<ConsoleLogEventBus> logger)
    {
        _logger = logger;
    }

    public Task Publish(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event published! Time: {CloudEventTime:O}, Type: {CloudEventType}", cloudEvent.Time, cloudEvent.Type);
        return Task.CompletedTask;
    }
}
