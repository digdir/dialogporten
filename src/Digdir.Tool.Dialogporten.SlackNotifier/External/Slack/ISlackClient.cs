namespace Digdir.Tool.Dialogporten.SlackNotifier.External.Slack;

internal interface ISlackClient
{
    Task SendAsync(SlackRequestDto message, CancellationToken cancellationToken);
}
