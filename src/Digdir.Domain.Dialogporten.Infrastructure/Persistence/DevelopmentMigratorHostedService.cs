using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class DevelopmentMigratorHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHostEnvironment _environment;

    public DevelopmentMigratorHostedService(IServiceScopeFactory serviceScopeFactory, IHostEnvironment environment)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DialogDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}