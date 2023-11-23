namespace Digdir.Tool.Dialogporten.SlackNotifier.External.Slack;

internal sealed class SlackRequestDto
{
    public required string ExceptionReport { get; init; }
    public required string Link { get; init; }
}