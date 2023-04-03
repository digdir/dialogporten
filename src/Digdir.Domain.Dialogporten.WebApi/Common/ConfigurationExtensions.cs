using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Digdir.Domain.Dialogporten.WebApi;

internal static class ConfigurationExtensions
{
    private const string AzureAppConfigurationUriConfigName = "AZURE_APPCONFIG_URI";

    public static IConfigurationBuilder AddAzureConfiguration(
        this ConfigurationManager config, 
        string? environment,
        TokenCredential? credential = null, 
        TimeSpan? refreshRate = null)
    {
        if (Uri.TryCreate(config[AzureAppConfigurationUriConfigName], UriKind.Absolute, out var appConfigUri))
        {
            credential ??= new DefaultAzureCredential();
            refreshRate ??= TimeSpan.FromMinutes(1);

            config.AddAzureAppConfiguration(appConfigOptions => appConfigOptions
                .Connect(appConfigUri, credential)
                .Select(KeyFilter.Any, LabelFilter.Null)
                .SelectIf(!string.IsNullOrWhiteSpace(environment), 
                    KeyFilter.Any, environment!)
                .ConfigureRefresh(refresh => refresh
                    .Register("Sentinel", refreshAll: true)
                    .SetCacheExpiration(refreshRate.Value))
                .ConfigureKeyVault(keyVaultOptions =>
                {
                    keyVaultOptions.SetCredential(credential);
                    keyVaultOptions.SetSecretRefreshInterval(refreshRate.Value);
                }));
        }

        return config;
    }

    private static AzureAppConfigurationOptions SelectIf(
        this AzureAppConfigurationOptions options,
        bool predicate,
        string keyFilter,
        string labelFilter = "\0") 
        => predicate ? options.Select(keyFilter, labelFilter) : options;
}