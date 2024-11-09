﻿using System.Collections.ObjectModel;
using System.Reflection;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.Interceptors;
using Digdir.Library.Entity.Abstractions.Features.Lookup;
using FluentAssertions;
using HotChocolate.Subscriptions;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using NSec.Cryptography;
using NSubstitute;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

public class DialogApplication : IAsyncLifetime
{
    private IMapper? _mapper;
    private Respawner _respawner = null!;
    private ServiceProvider _rootProvider = null!;
    private ServiceProvider _fixtureRootProvider = null!;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15.7")
        .Build();

    public async Task InitializeAsync()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(Assembly.GetAssembly(typeof(ApplicationSettings)));
        });
        _mapper = config.CreateMapper();

        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMicroseconds(1)))
                .WhenTypeIs<DateTimeOffset>();
            return options;
        });

        _fixtureRootProvider = _rootProvider = BuildServiceCollection().BuildServiceProvider();

        await _dbContainer.StartAsync();
        await EnsureDatabaseAsync();
        await BuildRespawnState();
    }

    /// <summary>
    /// This method lets you configure the IoC container for an integration test.
    /// It will be reset to the default configuration after each test.
    /// You may only call this, or equivalent methods once per test.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the method is called more than once per test.</exception>
    public void ConfigureServices(Action<IServiceCollection> configure)
    {
        if (_rootProvider != _fixtureRootProvider)
        {
            throw new InvalidOperationException($"Only one call to {nameof(ConfigureServices)} or equivalent methods are allowed per test.");
        }

        var serviceCollection = BuildServiceCollection();
        configure(serviceCollection);
        _rootProvider = serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// This method lets you configure the IoC container for an integration test.
    /// It will be reset to the default configuration after each test.
    /// You may only call this, or equivalent methods once per test.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the method is called more than once per test.</exception>
    public async Task<ITestHarness> ConfigureServicesWithMassTransitTestHarness(Action<IServiceCollection>? configure = null)
    {
        ConfigureServices(x =>
        {
            x.RemoveAll<IPublishEndpoint>()
                .AddMassTransitTestHarness(cfg =>
                {
                    var openTestEventConsumer = typeof(TestDomainEventConsumer<>);
                    foreach (var domainEventType in DomainExtensions.GetDomainEventTypes())
                    {
                        cfg.AddConsumer(openTestEventConsumer.MakeGenericType(domainEventType));
                    }
                });
            configure?.Invoke(x);
        });
        var harness = _rootProvider.GetRequiredService<ITestHarness>();
        await harness.Start();
        return harness;
    }

    private IServiceCollection BuildServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        return serviceCollection
            .AddApplication(Substitute.For<IConfiguration>(), Substitute.For<IHostEnvironment>())
            .AddDistributedMemoryCache()
            .AddLogging()
            .AddTransient<ConvertDomainEventsToOutboxMessagesInterceptor>()
            .AddTransient(x => new Lazy<IPublishEndpoint>(x.GetRequiredService<IPublishEndpoint>))
            .AddDbContext<DialogDbContext>((services, options) =>
                options.UseNpgsql(_dbContainer.GetConnectionString(), o =>
                    {
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    })
                    .AddInterceptors(services.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>()))
            .AddScoped<IDialogDbContext>(x => x.GetRequiredService<DialogDbContext>())
            .AddScoped<IResourceRegistry, LocalDevelopmentResourceRegistry>()
            .AddScoped<IServiceOwnerNameRegistry>(_ => CreateServiceOwnerNameRegistrySubstitute())
            .AddScoped<IPartyNameRegistry>(_ => CreateNameRegistrySubstitute())
            .AddScoped<IOptions<ApplicationSettings>>(_ => CreateApplicationSettingsSubstitute())
            .AddScoped<ITopicEventSender>(_ => Substitute.For<ITopicEventSender>())
            .AddScoped<IPublishEndpoint>(_ => Substitute.For<IPublishEndpoint>())
            .AddScoped<Lazy<ITopicEventSender>>(sp => new Lazy<ITopicEventSender>(() => sp.GetRequiredService<ITopicEventSender>()))
            .AddScoped<Lazy<IPublishEndpoint>>(sp => new Lazy<IPublishEndpoint>(() => sp.GetRequiredService<IPublishEndpoint>()))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IAltinnAuthorization, LocalDevelopmentAltinnAuthorization>()
            .AddSingleton<IUser, IntegrationTestUser>()
            .AddSingleton<ICloudEventBus, IntegrationTestCloudBus>()
            .Decorate<IUserResourceRegistry, LocalDevelopmentUserResourceRegistryDecorator>()
            .Decorate<IUserRegistry, LocalDevelopmentUserRegistryDecorator>();
    }

    private static IPartyNameRegistry CreateNameRegistrySubstitute()
    {
        var nameRegistrySubstitute = Substitute.For<IPartyNameRegistry>();

        nameRegistrySubstitute
            .GetName(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Brando Sando");

        return nameRegistrySubstitute;
    }

    private static string Base64UrlEncode(byte[] input) => Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").TrimEnd('=');

    private static IOptions<ApplicationSettings> CreateApplicationSettingsSubstitute()
    {
        var applicationSettingsSubstitute = Substitute.For<IOptions<ApplicationSettings>>();

        using var primaryKeyPair = Key.Create(SignatureAlgorithm.Ed25519,
            new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        var primaryPublicKey = primaryKeyPair.Export(KeyBlobFormat.RawPublicKey);
        var primaryPrivateKey = primaryKeyPair.Export(KeyBlobFormat.RawPrivateKey);

        using var secondaryKeyPair = Key.Create(SignatureAlgorithm.Ed25519,
            new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        var secondaryPublicKey = secondaryKeyPair.Export(KeyBlobFormat.RawPublicKey);
        var secondaryPrivateKey = secondaryKeyPair.Export(KeyBlobFormat.RawPrivateKey);

        applicationSettingsSubstitute
            .Value
            .Returns(new ApplicationSettings
            {
                Dialogporten = new DialogportenSettings
                {
                    BaseUri = new Uri("https://integration.test"),
                    Ed25519KeyPairs = new Ed25519KeyPairs
                    {
                        Primary = new Ed25519KeyPair
                        {
                            Kid = "integration-test-primary-signing-key",
                            PrivateComponent = Base64UrlEncode(primaryPrivateKey),
                            PublicComponent = Base64UrlEncode(primaryPublicKey)
                        },
                        Secondary = new Ed25519KeyPair
                        {
                            Kid = "integration-test-secondary-signing-key",
                            PrivateComponent = Base64UrlEncode(secondaryPrivateKey),
                            PublicComponent = Base64UrlEncode(secondaryPublicKey)
                        }
                    }
                }
            });

        return applicationSettingsSubstitute;
    }
    private static IServiceOwnerNameRegistry CreateServiceOwnerNameRegistrySubstitute()
    {
        var organizationRegistrySubstitute = Substitute.For<IServiceOwnerNameRegistry>();

        organizationRegistrySubstitute
            .GetServiceOwnerInfo(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ServiceOwnerInfo
            {
                OrgNumber = "991825827",
                ShortName = "digdir"
            });

        return organizationRegistrySubstitute;
    }

    public IMapper GetMapper() => _mapper!;

    public async Task DisposeAsync()
    {
        await _rootProvider.DisposeAsync();
        await _fixtureRootProvider.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    private async Task Publish(INotification notification)
    {
        using var scope = _rootProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
        await mediator.Publish(notification);
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _rootProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        return await mediator.Send(request);
    }

    public async Task ResetState()
    {
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
        if (_rootProvider != _fixtureRootProvider)
        {
            await _rootProvider.DisposeAsync();
            _rootProvider = _fixtureRootProvider;
        }
    }

    private async Task EnsureDatabaseAsync()
    {
        using var scope = _rootProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DialogDbContext>();
        await context.Database.MigrateAsync();
    }

    private async Task BuildRespawnState()
    {
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new()
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new[] { new Table("__EFMigrationsHistory") }
                .Concat(GetLookupTables())
                .ToArray()
        });
    }

    public List<CloudEvent> PopPublishedCloudEvents()
    {
        using var scope = _rootProvider.CreateScope();
        var cloudBus = scope.ServiceProvider.GetRequiredService<ICloudEventBus>() as IntegrationTestCloudBus;

        var events = cloudBus!.Events.ToList();
        cloudBus.Events.Clear();

        return events;
    }

    public async Task<List<T>> GetDbEntities<T>() where T : class
    {
        using var scope = _rootProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DialogDbContext>();
        return await db
            .Set<T>()
            .AsNoTracking()
            .ToListAsync();
    }

    private ReadOnlyCollection<Table> GetLookupTables()
    {
        using var scope = _rootProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DialogDbContext>();
        return db.Model.GetEntityTypes()
            .Where(x => typeof(ILookupEntity).IsAssignableFrom(x.ClrType))
            .Select(x => new Table(x.GetTableName()!))
            .ToList()
            .AsReadOnly();
    }
}
