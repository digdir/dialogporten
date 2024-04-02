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
using ZiggyCreatures.Caching.Fusion;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
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

        services.AddFusionCacheNewtonsoftJsonSerializer();

        if (infrastructureSettings.Redis.Enabled == true)
        {
            services.AddStackExchangeRedisCache(opt => opt.Configuration = infrastructureSettings.Redis.ConnectionString);
            services.AddFusionCacheStackExchangeRedisBackplane(opt => opt.Configuration = infrastructureSettings.Redis.ConnectionString);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // todo: do we need default cache? 🤔
        ConfigureFusionCache(services);
        ConfigureFusionCache(services, nameof(NameRegistryClient));
        ConfigureFusionCache(services, nameof(ResourceRegistryClient));
        ConfigureFusionCache(services, nameof(OrganizationRegistryClient));

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

    private static void ConfigureFusionCache(IServiceCollection services, string? cacheName = null)
    {
        // todo: consider open telemetry?
        var fusionCacheConfig = services.AddFusionCache(cacheName ?? string.Empty)
            .WithOptions(options =>
            {
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(2);
                // todo: Consider log levels based on environment. More debug for dev, less for prod.
                options.FailSafeActivationLogLevel = LogLevel.Debug;
                options.SerializationErrorsLogLevel = LogLevel.Warning;
                options.DistributedCacheSyntheticTimeoutsLogLevel = LogLevel.Debug;
                options.DistributedCacheErrorsLogLevel = LogLevel.Error;
                options.FactorySyntheticTimeoutsLogLevel = LogLevel.Debug;
                options.FactoryErrorsLogLevel = LogLevel.Error;
            })
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                // todo: Let's discuss these settings..
                Duration = TimeSpan.FromDays(1),

                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromHours(2),
                FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

                FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
                FactoryHardTimeout = TimeSpan.FromMilliseconds(1500),

                DistributedCacheSoftTimeout = TimeSpan.FromSeconds(1),
                DistributedCacheHardTimeout = TimeSpan.FromSeconds(2),

                AllowBackgroundDistributedCacheOperations = true,

                JitterMaxDuration = TimeSpan.FromSeconds(2)
            })
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();
    }
}
