using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class ConfigurationExtensions
{
    private const string AppsettingsLocalJson = "appsettings.local.json";

    public static IConfigurationBuilder AddLocalConfiguration(this IConfigurationBuilder builder, IHostEnvironment environment)
        => !environment.IsDevelopment() ? builder : builder.AddJsonFile(AppsettingsLocalJson, optional: true, reloadOnChange: true);
}
