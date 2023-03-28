using Azure.Core;
using Azure.Identity;

namespace Digdir.Domain.Dialogporten.WebApi;

internal static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddAzureConfiguration(
        this ConfigurationManager config, 
        TokenCredential? credential = null, 
        TimeSpan? refreshRate = null)
    {
        credential ??= new DefaultAzureCredential();
        refreshRate ??= TimeSpan.FromMinutes(1);

        if (Uri.TryCreate(config["AZURE_APPCONFIG_URI"], UriKind.Absolute, out var appConfigUri))
        {
            config.AddAzureAppConfiguration(appConfigOptions =>
            {
                var refreshRate = TimeSpan.FromMinutes(1);
                appConfigOptions
                    .Connect(appConfigUri, credential)
                    //.ConfigureRefresh(x => x.SetCacheExpiration(refreshRate))
                    .ConfigureKeyVault(keyVaultOptions =>
                    {
                        keyVaultOptions.SetCredential(credential);
                        keyVaultOptions.SetSecretRefreshInterval(refreshRate);
                    });
            });
        }

        return config;
    }
}