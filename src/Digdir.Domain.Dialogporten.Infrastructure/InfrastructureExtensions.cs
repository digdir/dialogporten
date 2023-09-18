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
using Polly.Extensions.Http;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Microsoft.Extensions.Caching.Distributed;
using Polly.Caching.Distributed;
using Polly.Caching;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class SystemJsonSerializer<TResult> : ICacheItemSerializer<TResult, string>
{
    public TResult Deserialize(string objectToDeserialize) => JsonSerializer.Deserialize<TResult>(objectToDeserialize)!;
    public string Serialize(TResult objectToSerialize) => JsonSerializer.Serialize(objectToSerialize);
}

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        services.AddPolicyRegistry((services, registry) =>
        {
            registry.Add("DefaultHttpRetryPolicy", HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3)));

            registry.Add("OrgResourceReferenceCache", Policy.CacheAsync<string>((IAsyncCacheProvider)services
                .GetRequiredService<IDistributedCache>()
                .AsAsyncCacheProvider<string>()
                .WithSerializer(new SystemJsonSerializer<OrgResourceReference>()), 
                TimeSpan.FromMinutes(5)));
        });

        return services
            // Settings
            .Configure<InfrastructureSettings>(configurationSection)

            // Framework
            .AddDistributedMemoryCache()
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
            .AddTransient<IResourceRegistry, ResourceRegistery>()

            // Decorate
            .Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>))

            // HttpClient
            .AddMaskinportenHttpClient<ICloudEventBus, AltinnEventsClient, SettingsJwkClientDefinition>(configurationSection, x => x.ClientSettings.ExhangeToAltinnToken = true)
                .Services
            .AddHttpClient<ResourceRegistryClient>((services, client) => client.BaseAddress = services.GetRequiredService<InfrastructureSettings>().Altinn.BaseUri)
                .AddPolicyHandlerFromRegistry("DefaultHttpRetryPolicy")
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
        var settings = configuration.Get<InfrastructureSettings>();
        services.RegisterMaskinportenClientDefinition<TClientDefinition>(typeof(TClient)!.FullName, settings.MaskinportenSettings);
        return services.AddHttpClient<TClient, TImplementation>().AddMaskinportenHttpMessageHandler<TClientDefinition, TClient>(configureClientDefinition);
    }
}
