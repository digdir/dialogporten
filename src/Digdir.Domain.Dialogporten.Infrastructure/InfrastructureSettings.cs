using Altinn.ApiClients.Maskinporten.Config;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public sealed class InfrastructureSettings
{
    public const string ConfigurationSectionName = "Infrastructure";

    public required string DialogDbConnectionString { get; init; }
    public required AltinnPlatformSettings Altinn { get; init; }
    public required MaskinportenSettings MaskinportenSettings { get; init; }
}

public sealed class AltinnPlatformSettings
{
    public required Uri BaseUri { get; init; }
}