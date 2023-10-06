using Microsoft.Extensions.Configuration;

namespace Digdir.Domain.Dialogporten.Application;

public sealed class LocalDevelopmentSettings
{
    public const string ConfigurationSectionName = "LocalDevelopment";
    public bool UseLocalDevelopmentUser { get; init; } = true;
    public bool UseLocalDevelopmentResourceRegister { get; init; } = true;
    public bool UseLocalDevelopmentCloudEventBus { get; init; } = true;
    public bool DisableShortCircuitOutboxDispatcher { get; init; } = true;
    public bool DisableAuth { get; init; } = true;
}

public static class LocalDevelopmentSettingsExtensions
{
    public static LocalDevelopmentSettings GetLocalDevelopmentSettings(this IConfiguration configuration) =>
        configuration
            .GetSection(LocalDevelopmentSettings.ConfigurationSectionName)
            .Get<LocalDevelopmentSettings>()
            ?? new();
}