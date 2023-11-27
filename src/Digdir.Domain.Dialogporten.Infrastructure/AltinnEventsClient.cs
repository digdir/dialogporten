using System.Net.Http.Headers;
using System.Net.Http.Json;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal class AltinnEventsClient : ICloudEventBus
{
    private readonly HttpClient _client;

    public AltinnEventsClient(HttpClient client)
    {
        _client = client;
    }

    public async Task Publish(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(_client.BaseAddress!) { Path = "/events/api/v1/events" };
        var msg = new HttpRequestMessage(HttpMethod.Post, uriBuilder.Uri)
        {
            Content = JsonContent.Create(cloudEvent, options: SerializerOptions.CloudEventSerializerOptions)
        };
        msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/cloudevents+json");
        var response = await _client.SendAsync(msg, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
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
