namespace Digdir.Tool.Dialogporten.SlackNotifier.External.Slack;

internal sealed class SlackOptions
{
    public const string ConfigurationSection = "Slack";

    public required string WebhookUrl { get; init; }
}