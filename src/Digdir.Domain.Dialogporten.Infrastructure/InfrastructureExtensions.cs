using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly.Extensions.Http;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Digdir.Domain.Dialogporten.Infrastructure.Common;
using FluentValidation;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.Events;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.NameRegistry;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddPolicyRegistry((services, registry) =>
        {
            registry.Add(PollyPolicy.DefaultHttpRetryPolicy, HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3)));
        });

        var infrastructureConfigurationSection = configuration.GetSection(InfrastructureSettings.ConfigurationSectionName);
        services.AddOptions<InfrastructureSettings>()
            .Bind(infrastructureConfigurationSection)
            .ValidateFluently()
            .ValidateOnStart();

        var thisAssembly = Assembly.GetExecutingAssembly();

        services
            // Framework
            .AddValidatorsFromAssembly(thisAssembly, ServiceLifetime.Transient, includeInternalTypes: true);

        var infrastructureSettings = infrastructureConfigurationSection.Get<InfrastructureSettings>()
                    ?? throw new InvalidOperationException("Failed to get Redis settings. Infrastructure settings must not be null.");

        services.AddFusionCacheNeueccMessagePackSerializer();

        if (infrastructureSettings.Redis.Enabled == true)
        {
            services.AddStackExchangeRedisCache(opt => opt.Configuration = infrastructureSettings.Redis.ConnectionString);
            services.AddFusionCacheStackExchangeRedisBackplane(opt => opt.Configuration = infrastructureSettings.Redis.ConnectionString);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.ConfigureFusionCache(nameof(Altinn.NameRegistry), new()
        {
            Duration = TimeSpan.FromHours(24),
            FailSafeMaxDuration = TimeSpan.FromHours(26)
        })
        .ConfigureFusionCache(nameof(Altinn.ResourceRegistry), new()
        {
            Duration = TimeSpan.FromMinutes(20),
            // The resource list is several megabytes and might take a while to process
            FactoryHardTimeout = TimeSpan.FromSeconds(10)
        })
        .ConfigureFusionCache(nameof(Altinn.OrganizationRegistry), new()
        {
            Duration = TimeSpan.FromHours(24),
            FailSafeMaxDuration = TimeSpan.FromHours(26)
        })
        .ConfigureFusionCache(nameof(Altinn.Authorization), new()
        {
            // This cache has high cardinality, and will create several keys per user:
            // - One entry per user per dialog search and combination of party and service resource parameters
            // - One entry per dialog details
            // EU systems iterating over several parties in search view and fetching details for each dialog will
            // potentially create hundreds over even thousands of cache entries within the cache TTL. To avoid
            // memory exhaustion, we therefore disable the memory cache for authorization results, and rely solely on
            // the distributed cache.
            SkipMemoryCache = true,
            // In normal operations, 15 minutes delay is deemed acceptable for authorization data
            Duration = TimeSpan.FromMinutes(15),
            // In case Altinn Authorization is down/overloaded, we allow the re-usage of stale authorization data
            // for an additional 15 minutes. Using default FailSafeThrottleDuration.
            FailSafeMaxDuration = TimeSpan.FromMinutes(30),
            // If the request to Altinn Authorization takes too long, we allow the cache to return stale data
            // temporarily whilst updating the cache in the background. Note that we are also using eager refresh
            // and a backplane.
            FactorySoftTimeout = TimeSpan.FromSeconds(2),
            // Timeout for the cache to wait for the factory to complete, which when reached without fail-safe data
            // will cause an exception to be thrown
            FactoryHardTimeout = TimeSpan.FromSeconds(10)
        });

        services.AddDbContext<DialogDbContext>((services, options) =>
            {
                var connectionString = services.GetRequiredService<IOptions<InfrastructureSettings>>()
                    .Value.DialogDbConnectionString;
                options.UseNpgsql(connectionString, o =>
                    {
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    })
                    .AddInterceptors(services.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
            })
            .AddHostedService<DevelopmentMigratorHostedService>()

            // Scoped
            .AddScoped<IDialogDbContext>(x => x.GetRequiredService<DialogDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>()

            // Transient
            .AddTransient<OutboxDispatcher>()
            .AddTransient<ConvertDomainEventsToOutboxMessagesInterceptor>()

            // Decorate
            .Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

        // HttpClient
        services.
            AddMaskinportenHttpClient<ICloudEventBus, AltinnEventsClient, SettingsJwkClientDefinition>(
                infrastructureConfigurationSection,
                x => x.ClientSettings.ExhangeToAltinnToken = true)
            .ConfigureHttpClient((services, client) =>
            {
                client.BaseAddress = services.GetRequiredService<IOptions<InfrastructureSettings>>().Value.Altinn.EventsBaseUri;
            });
        services.AddHttpClient<IResourceRegistry, ResourceRegistryClient>((services, client) =>
                client.BaseAddress = services.GetRequiredService<IOptions<InfrastructureSettings>>().Value.Altinn.BaseUri)
            .AddPolicyHandlerFromRegistry(PollyPolicy.DefaultHttpRetryPolicy);

        services.AddHttpClient<IOrganizationRegistry, OrganizationRegistryClient>((services, client) =>
                client.BaseAddress = services.GetRequiredService<IOptions<InfrastructureSettings>>().Value.AltinnCdn.BaseUri)
            .AddPolicyHandlerFromRegistry(PollyPolicy.DefaultHttpRetryPolicy);

        services.AddMaskinportenHttpClient<INameRegistry, NameRegistryClient, SettingsJwkClientDefinition>(
                infrastructureConfigurationSection,
                x => x.ClientSettings.ExhangeToAltinnToken = true)
            .ConfigureHttpClient((services, client) =>
            {
                var altinnSettings = services.GetRequiredService<IOptions<InfrastructureSettings>>().Value.Altinn;
                client.BaseAddress = altinnSettings.BaseUri;
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", altinnSettings.SubscriptionKey);
            })
            .AddPolicyHandlerFromRegistry(PollyPolicy.DefaultHttpRetryPolicy);

        services.AddMaskinportenHttpClient<IAltinnAuthorization, AltinnAuthorizationClient, SettingsJwkClientDefinition>(
                infrastructureConfigurationSection,
                x => x.ClientSettings.ExhangeToAltinnToken = true)
            .ConfigureHttpClient((services, client) =>
            {
                var altinnSettings = services.GetRequiredService<IOptions<InfrastructureSettings>>().Value.Altinn;
                client.BaseAddress = altinnSettings.BaseUri;
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", altinnSettings.SubscriptionKey);
            })
            .AddPolicyHandlerFromRegistry(PollyPolicy.DefaultHttpRetryPolicy);

        if (environment.IsDevelopment())
        {
            var localDeveloperSettings = configuration.GetLocalDevelopmentSettings();
            services
                .ReplaceTransient<ICloudEventBus, ConsoleLogEventBus>(predicate: localDeveloperSettings.UseLocalDevelopmentCloudEventBus)
                .ReplaceTransient<IResourceRegistry, LocalDevelopmentResourceRegistry>(predicate: localDeveloperSettings.UseLocalDevelopmentResourceRegister)
                .ReplaceTransient<IAltinnAuthorization, LocalDevelopmentAltinnAuthorization>(predicate: localDeveloperSettings.UseLocalDevelopmentAltinnAuthorization);
        }

        return services;
    }

    public class FusionCacheSettings
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan FailSafeMaxDuration { get; set; } = TimeSpan.FromHours(2);
        public TimeSpan FailSafeThrottleDuration { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan FactorySoftTimeout { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan FactoryHardTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan DistributedCacheSoftTimeout { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DistributedCacheHardTimeout { get; set; } = TimeSpan.FromSeconds(2);
        public bool AllowBackgroundDistributedCacheOperations { get; set; } = true;
        public bool IsFailSafeEnabled { get; set; } = true;
        public TimeSpan JitterMaxDuration { get; set; } = TimeSpan.FromSeconds(2);
        public float EagerRefreshThreshold { get; set; } = 0.8f;
        public bool SkipMemoryCache { get; set; }
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
        services.RegisterMaskinportenClientDefinition<TClientDefinition>(typeof(TClient)!.FullName, settings!.Maskinporten);
        return services
            .AddHttpClient<TClient, TImplementation>()
            .AddMaskinportenHttpMessageHandler<TClientDefinition, TClient>(configureClientDefinition);
    }

    private static IServiceCollection ConfigureFusionCache(this IServiceCollection services, string cacheName, FusionCacheSettings? settings = null)
    {
        settings ??= new FusionCacheSettings();

        services.AddFusionCache(cacheName)
            .WithOptions(options =>
            {
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(2);
            })
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = settings.Duration,

                IsFailSafeEnabled = settings.IsFailSafeEnabled,
                FailSafeMaxDuration = settings.FailSafeMaxDuration,
                FailSafeThrottleDuration = settings.FailSafeThrottleDuration,

                FactorySoftTimeout = settings.FactorySoftTimeout,
                FactoryHardTimeout = settings.FactoryHardTimeout,

                DistributedCacheSoftTimeout = settings.DistributedCacheSoftTimeout,
                DistributedCacheHardTimeout = settings.DistributedCacheHardTimeout,

                AllowBackgroundDistributedCacheOperations = settings.AllowBackgroundDistributedCacheOperations,

                JitterMaxDuration = settings.JitterMaxDuration,
                EagerRefreshThreshold = settings.EagerRefreshThreshold,

                SkipMemoryCache = settings.SkipMemoryCache
            })
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        return services;
    }
}
