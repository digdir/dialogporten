using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.Abstractions.Features.Lookup;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Xunit;

namespace Digdir.Domain.Dialogporten.Application.Tests.Integration.Common;

public class DialogueApplication : IAsyncLifetime
{
    private Respawner _respawner = null!;
    private ServiceProvider _rootProvider = null!;
    private readonly TestcontainerDatabase _dbContainer =
        new ContainerBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration("postgres:alpine3.17")
            {
                Database = "db",
                Username = "postgres",
                Password = "postgres",
            })
            .Build();

    public async Task InitializeAsync()
    {
        _rootProvider = new ServiceCollection()
            .AddApplication(applicationSettings =>
            {

            })
            .AddDbContext<DialogueDbContext>(x => x.UseNpgsql(_dbContainer.ConnectionString))
            .AddScoped<IDomainEventPublisher, DomainEventPublisher>()
            .AddScoped<IDialogueDbContext>(x => x.GetRequiredService<DialogueDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .BuildServiceProvider();
        await _dbContainer.StartAsync();
        await EnsureDatabaseAsync();
        await BuildRespawnState();
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
        await using var connection = new NpgsqlConnection(_dbContainer.ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    private async Task EnsureDatabaseAsync()
    {
        using var scope = _rootProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DialogueDbContext>();
        await context.Database.MigrateAsync();
    }

    private async Task BuildRespawnState()
    {
        await using var connection = new NpgsqlConnection(_dbContainer.ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new()
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new[] { new Respawn.Graph.Table("__EFMigrationsHistory") }
                .Concat(GetLookupTables())
                .ToArray()
        });
    }

    private IEnumerable<Respawn.Graph.Table> GetLookupTables()
    {
        using var scope = _rootProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DialogueDbContext>();
        return db.Model.GetEntityTypes()
            .Where(x => typeof(ILookupEntity).IsAssignableFrom(x.ClrType))
            .Select(x => new Respawn.Graph.Table(x.GetTableName()!))
            .ToList();
    }
}
