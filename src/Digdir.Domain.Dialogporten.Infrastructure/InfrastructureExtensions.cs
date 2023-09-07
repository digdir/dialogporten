using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);
        return services
            // Settings
            .Configure<InfrastructureSettings>(configurationSection)

            // Framework
            .AddDbContext<DialogDbContext>((services, options) =>
            {
                var connectionString = services
                    .GetRequiredService<IOptions<InfrastructureSettings>>()
                    .Value.DialogDbConnectionString;
                options.UseNpgsql(connectionString)
                    .AddInterceptors(services.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
            })
            .AddHostedService<DevelopmentMigratorHostedService>()
            //.AddHostedService<OutboxScheduler>()

            // Singleton

            // Scoped
            .AddScoped<DomainEventPublisher>()
            .AddScoped<IDomainEventPublisher>(x => x.GetRequiredService<DomainEventPublisher>())
            .AddScoped<IDialogDbContext>(x => x.GetRequiredService<DialogDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>()

            // Transient
            .AddTransient<OutboxDispatcher>()
            .AddTransient<ConvertDomainEventsToOutboxMessagesInterceptor>()
            .AddTransient<ICloudEventBus, AltinnEventsClient>()

            // Decorate
            .Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>))

            // HttpClient
            .AddMaskinportenHttpClient<ICloudEventBus, AltinnEventsClient, SettingsJwkClientDefinition>(configurationSection, x => x.ClientSettings.ExhangeToAltinnToken = true)
                .Services
            .AddHttpClient<IResourceRegistry, ResourceRegistery>()
                .Services
            ;
    }

    private static IHttpClientBuilder AddMaskinportenHttpClient<TClient, TImplementation, TClientDefinition>(
        this IServiceCollection services, 
        IConfiguration configuration, 
        Action<TClientDefinition>? configureClientDefinition = null) 
        where TClient : class
        where TImplementation : class, TClient
        where TClientDefinition : class, IClientDefinition
    {
        var settings = configuration.GetSection("MaskinportenSettings").Get<MaskinportenSettings>();
        services.RegisterMaskinportenClientDefinition<TClientDefinition>(typeof(TClient)!.FullName, settings);
        return services.AddHttpClient<TClient, TImplementation>().AddMaskinportenHttpMessageHandler<TClientDefinition, TClient>(configureClientDefinition);
    }
}
