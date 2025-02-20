using System.CodeDom.Compiler;
using System.Reflection;
using Altinn.ApiClients.Dialogporten.Common;
using Altinn.ApiClients.Dialogporten.Infrastructure;
using Altinn.ApiClients.Dialogporten.Services;
using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Refit;

namespace Altinn.ApiClients.Dialogporten;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDialogportenClient(this IServiceCollection services, DialogportenSettings settings)
    {
        if (!DialogportenSettings.Validate())
        {
            throw new InvalidOperationException("Invalid configuration");
        }
        services.TryAddSingleton<IOptions<DialogportenSettings>>(new OptionsWrapper<DialogportenSettings>(settings));

        services.RegisterMaskinportenClientDefinition<SettingsJwkClientDefinition>("dialogporten-sp-sdk", settings.Maskinporten);

        var refitClients = AssemblyMarker.Assembly.GetTypes()
            .Where(x =>
                x.IsInterface &&
                x.GetCustomAttribute<GeneratedCodeAttribute>()?.Tool == "Refitter")
            .ToList();

        foreach (var refitClient in refitClients)
        {
            services
                .AddRefitClient(refitClient)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(settings.BaseUri))
                .AddMaskinportenHttpMessageHandler<SettingsJwkClientDefinition>("dialogporten-sp-sdk");
        }

        services
            .AddRefitClient<IInternalDialogportenApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(settings.BaseUri));

        services.AddHostedService<EdDsaSecurityKeysCacheService>();
        services.TryAddTransient<IDialogTokenValidator, DialogTokenValidator>();
        services.TryAddSingleton<DefaultEdDsaSecurityKeysCache>();
        services.TryAddSingleton<IEdDsaSecurityKeysCache>(x => x.GetRequiredService<DefaultEdDsaSecurityKeysCache>());
        services.TryAddTransient<IClock, DefaultClock>();
        return services;
    }

    public static IServiceCollection AddDialogportenClient(this IServiceCollection services, Action<DialogportenSettings> configureOptions)
    {
        var dialogportenSettings = new DialogportenSettings();
        configureOptions.Invoke(dialogportenSettings);
        return services.AddDialogportenClient(dialogportenSettings);
    }
}
