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
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;
using System.Text.Json;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.NameRegistry;

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

        ConfigureFusionCache(services, nameof(Altinn.NameRegistry), new()
        {
            Duration = TimeSpan.FromDays(1),
        });
        ConfigureFusionCache(services, nameof(Altinn.ResourceRegistry), new()
        {
            Duration = TimeSpan.FromMinutes(20),
        });
        ConfigureFusionCache(services, nameof(Altinn.OrganizationRegistry), new()
        {
            Duration = TimeSpan.FromDays(1),
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
            // TODO! Add cache policy based on request body
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
        public TimeSpan FactorySoftTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan FactoryHardTimeout { get; set; } = TimeSpan.FromMilliseconds(1500);
        public TimeSpan DistributedCacheSoftTimeout { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DistributedCacheHardTimeout { get; set; } = TimeSpan.FromSeconds(2);
        public bool AllowBackgroundDistributedCacheOperations { get; set; } = true;
        public bool IsFailSafeEnabled { get; set; } = true;
        public TimeSpan JitterMaxDuration { get; set; } = TimeSpan.FromSeconds(2);
        public float EagerRefreshThreshold { get; set; } = 0.8f;
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

    private static void ConfigureFusionCache(IServiceCollection services, string cacheName, FusionCacheSettings? settings = null)
    {
        settings ??= new FusionCacheSettings();

        // todo: consider open telemetry?
        var fusionCacheConfig = services.AddFusionCache(cacheName)
            .WithOptions(options =>
            {
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(2);
            })
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                // todo: Let's discuss these settings..
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
                EagerRefreshThreshold = settings.EagerRefreshThreshold
            })
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();
    }
}
