using System.Collections.ObjectModel;
using System.Text.Json;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.Abstractions.Features.Lookup;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using NSubstitute;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

public class DialogApplication : IAsyncLifetime
{
    private Respawner _respawner = null!;
    private ServiceProvider _rootProvider = null!;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15.4")
        .Build();

    public async Task InitializeAsync()
    {
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            //options.ExcludingMissingMembers();
            options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMicroseconds(1)))
                .WhenTypeIs<DateTimeOffset>();
            return options;
        });

        var serviceCollection = new ServiceCollection();

        _rootProvider = serviceCollection
            .AddApplication(Substitute.For<IConfiguration>(), Substitute.For<IHostEnvironment>())
            .AddDistributedMemoryCache()
            .AddTransient<ConvertDomainEventsToOutboxMessagesInterceptor>()
            .AddDbContext<DialogDbContext>((services, options) =>
                options.UseNpgsql(_dbContainer.GetConnectionString(), o =>
                {
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .AddInterceptors(services.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>()))
            .AddScoped<IDialogDbContext>(x => x.GetRequiredService<DialogDbContext>())
            .AddScoped<IUser, IntegrationTestUser>()
            .AddScoped<IResourceRegistry, LocalDevelopmentResourceRegistry>()
            .AddScoped<IServiceOwnerNameRegistry>(_ => CreateOrganizationRegistrySubstitute())
            .AddScoped<IPartyNameRegistry>(_ => CreateNameRegistrySubstitute())
            .AddScoped<IOptions<ApplicationSettings>>(_ => CreateApplicationSettingsSubstitute())
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddSingleton<ICloudEventBus, IntegrationTestCloudBus>()
            .Decorate<IUserResourceRegistry, LocalDevelopmentUserResourceRegistryDecorator>()
            .Decorate<IUserRegistry, LocalDevelopmentUserRegistryDecorator>()
            .BuildServiceProvider();

        await _dbContainer.StartAsync();
        await EnsureDatabaseAsync();
        await BuildRespawnState();
    }

    private static IPartyNameRegistry CreateNameRegistrySubstitute()
    {
        var nameRegistrySubstitute = Substitute.For<IPartyNameRegistry>();

        nameRegistrySubstitute
            .GetPersonName(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Brando Sando");

        return nameRegistrySubstitute;
    }

    private static IOptions<ApplicationSettings> CreateApplicationSettingsSubstitute()
    {
        var applicationSettingsSubstitute = Substitute.For<IOptions<ApplicationSettings>>();

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
                            Kid = "kid1",
                            PrivateComponent = "private1",
                            PublicComponent = "public1"
                        },
                        Secondary = new Ed25519KeyPair
                        {
                            Kid = "kid2",
                            PrivateComponent = "private2",
                            PublicComponent = "public2"
                        }
                    }
                }
            });

        return applicationSettingsSubstitute;
    }
    private static IServiceOwnerNameRegistry CreateOrganizationRegistrySubstitute()
    {
        var organizationRegistrySubstitute = Substitute.For<IServiceOwnerNameRegistry>();

        organizationRegistrySubstitute
            .GetServiceOwnerShortName(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("digdir");

        return organizationRegistrySubstitute;
    }

    public async Task DisposeAsync()
    {
        await _rootProvider.DisposeAsync();
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

    public async Task PublishOutBoxMessages()
    {
        var outBoxMessages = await GetDbEntities<OutboxMessage>();
        var eventAssembly = typeof(OutboxMessage).Assembly;
        foreach (var outboxMessage in outBoxMessages)
        {
            var eventType = eventAssembly.GetType(outboxMessage.EventType);
            var domainEvent = JsonSerializer.Deserialize(outboxMessage.EventPayload, eventType!) as INotification;
            await Publish(domainEvent!);
        }
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
