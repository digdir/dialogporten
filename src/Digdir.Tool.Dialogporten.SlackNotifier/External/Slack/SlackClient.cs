using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Digdir.Tool.Dialogporten.SlackNotifier.External.Slack;

internal sealed class SlackClient : ISlackClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SlackOptions> _slackOptions;

    public SlackClient(HttpClient httpClient, IOptions<SlackOptions> slackOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _slackOptions = slackOptions ?? throw new ArgumentNullException(nameof(slackOptions));
    }

    public async Task SendAsync(SlackRequestDto message, CancellationToken cancellationToken)
    {
        // TEMP DEBUG
        Console.WriteLine($"Slack WebhookURL: {_slackOptions.Value.WebhookUrl}");
        if (!Uri.TryCreate(_slackOptions.Value.WebhookUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        await _httpClient.PostAsJsonAsync(uri, message, cancellationToken);
    }
}
