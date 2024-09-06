using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class ConfigurationExtensions
{
    private const string AppsettingsLocalJson = "appsettings.local.json";


    /// <summary>
    /// Adds the local configuration file (appsettings.local.json) to the configuration builder if the environment is Development.
    /// </summary>
    /// <param name="builder">The configuration builder to add the local configuration file to.</param>
    /// <param name="environment">The host environment to check if it is in development.</param>
    /// <returns>The configuration builder with the local configuration file added if in development.</returns>
    public static IConfigurationBuilder AddLocalConfiguration(this IConfigurationBuilder builder, IHostEnvironment environment)
        => !environment.IsDevelopment() ? builder : builder.AddJsonFile(AppsettingsLocalJson, optional: true, reloadOnChange: true);
}
