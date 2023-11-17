using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Digdir.Tool.Dialogporten.SlackNotifier.External.Slack;

internal sealed class SlackClient : ISlackClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SlackOptions> _slackOptions;

    public SlackClient(HttpClient httpClient, IOptions<SlackOptions> slackOptions)
    {
        _httpClient = httpClient;
        _slackOptions = slackOptions;
    }

    public Task SendAsync(SlackRequestDto message, CancellationToken cancellationToken) =>
        _httpClient.PostAsJsonAsync(_slackOptions.Value.WebhookUrl, message, cancellationToken);
}
