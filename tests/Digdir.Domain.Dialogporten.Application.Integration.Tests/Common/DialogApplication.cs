using System.Collections.ObjectModel;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
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
            .AddScoped<IOrganizationRegistry>(_ => CreateOrganizationRegistrySubstitute())
            .AddScoped<INameRegistry>(_ => CreateNameRegistrySubstitute())
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .Decorate<IUserService, LocalDevelopmentUserServiceDecorator>()
            .BuildServiceProvider();

        await _dbContainer.StartAsync();
        await EnsureDatabaseAsync();
        await BuildRespawnState();
    }

    private static INameRegistry CreateNameRegistrySubstitute()
    {
        var nameRegistrySubstitute = Substitute.For<INameRegistry>();

        nameRegistrySubstitute
            .GetName(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Brando Sando");

        return nameRegistrySubstitute;
    }

    private static IOrganizationRegistry CreateOrganizationRegistrySubstitute()
    {
        var organizationRegistrySubstitute = Substitute.For<IOrganizationRegistry>();

        organizationRegistrySubstitute
            .GetOrgShortName(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("digdir");

        return organizationRegistrySubstitute;
    }

    public async Task DisposeAsync()
    {
        await _rootProvider.DisposeAsync();
        await _dbContainer.DisposeAsync();
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

    public List<T> GetDbEntities<T>() where T : class
    {
        using var scope = _rootProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DialogDbContext>();
        return db.Set<T>().ToList();
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
