using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Diagnostics.CodeAnalysis;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

/// <summary>
/// Wrapper around azure app configuration bootstrapping such that azure app
/// config is activated through the environment variable AZURE_APPCONFIG_URI.
/// </summary>
internal static class AzureAppConfigurationExtensions
{
    private const string AzureAppConfigurationUriConfigName = "AZURE_APPCONFIG_URI";
    private const string SentinelKey = "Sentinel";

    public static IConfigurationBuilder AddAzureConfiguration(
        this ConfigurationManager config,
        string? environment,
        TokenCredential? credential = null,
        TimeSpan? refreshRate = null)
    {
        if (!config.TryGetAzureAppConfigUri(out var appConfigUri))
        {
            return config;
        }

        credential ??= new DefaultAzureCredential();
        refreshRate ??= TimeSpan.FromMinutes(1);

        return config.AddAzureAppConfiguration(appConfigOptions => appConfigOptions
            .Connect(appConfigUri, credential)
            .Select(KeyFilter.Any, LabelFilter.Null)
            .SelectIf(!string.IsNullOrWhiteSpace(environment),
                keyFilter: KeyFilter.Any,
                labelFilter: environment!)
            .ConfigureRefresh(refresh => refresh
                .Register(SentinelKey, refreshAll: true)
                .SetCacheExpiration(refreshRate.Value))
            .ConfigureKeyVault(keyVaultOptions =>
            {
                keyVaultOptions.SetCredential(credential);
                keyVaultOptions.SetSecretRefreshInterval(refreshRate.Value);
            }));
    }

    public static IApplicationBuilder UseAzureConfiguration(this IApplicationBuilder builder)
    {
        // Check to see if we are targeting an instance of azure app
        // configuration before using azure app configuration middleware.
        return builder.ApplicationServices
            .GetRequiredService<IConfiguration>()
            .TryGetAzureAppConfigUri(out _)
                ? builder.UseAzureAppConfiguration()
                : builder;
    }

    private static AzureAppConfigurationOptions SelectIf(
        this AzureAppConfigurationOptions options,
        bool predicate,
        string keyFilter,
        string labelFilter = "\0")
        => predicate ? options.Select(keyFilter, labelFilter) : options;

    private static bool TryGetAzureAppConfigUri(this IConfiguration config, [NotNullWhen(true)] out Uri? uri)
    {
        var uriAsString = config[AzureAppConfigurationUriConfigName];

        if (string.IsNullOrWhiteSpace(uriAsString))
        {
            uri = null;
            return false;
        }

        if (!Uri.TryCreate(uriAsString, UriKind.Absolute, out uri))
        {
            throw new ArgumentException(
                $"Invalid {AzureAppConfigurationUriConfigName} value: {uriAsString}. " +
                $"Expected null or whitespace for environments not targeting an " +
                $"instance of azure AppConfiguration (usually local development) " +
                $"or a valid absolute uri pointing to an instance of azure " +
                $"AppConfiguration.");
        }

        return true;
    }
}
