namespace Digdir.Domain.Dialogporten.Infrastructure;

public sealed class InfrastructureSettings
{
    public const string ConfigurationSectionName = "InfrastructureSettings";

    public string DialogueDbConnectionString { get; set; } = string.Empty;
}
