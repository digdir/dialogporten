using Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
        // TODO: Config Altinn endpoint
        var msg = new HttpRequestMessage(HttpMethod.Post, "https://platform.tt02.altinn.no/events/api/v1/events")
        {
            Content = JsonContent.Create(cloudEvent)
        };
        msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/cloudevents+json");
        var response = await _client.SendAsync(msg, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
