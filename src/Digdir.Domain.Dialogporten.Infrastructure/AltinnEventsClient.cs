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
        var msg = new HttpRequestMessage(HttpMethod.Post, "https://platform.tt02.altinn.no/events/api/v1/events")
        {
            //Headers =
            //{
            //    { "Accept", "text/plain" },
            //    { "Content-Type", "application/json" }
            //},
            Content = JsonContent.Create(cloudEvent)
        };
        msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _client.SendAsync(msg, cancellationToken);
        var lala = await response.Content.ReadAsStringAsync();

        //var response = await _client.PostAsJsonAsync(
        //    "https://platform.tt02.altinn.no/events/api/v1/events",
        //    cloudEvent,
        //    cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
