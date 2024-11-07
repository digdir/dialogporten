using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Development;

/// <summary>
/// This hosted service is only active in development environments. It cleans up the
/// outbox/event tables. However, it keeps the last 12 hours of data to allow for
/// debugging throughout one workday.
/// </summary>
internal sealed class DevelopmentCleanupOutboxHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHostEnvironment _environment;

    public DevelopmentCleanupOutboxHostedService(IServiceScopeFactory serviceScopeFactory, IHostEnvironment environment)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.ShouldRunDevelopmentHostedService())
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DialogDbContext>();
        var oldestAllowed = DateTimeOffset.UtcNow.AddHours(-12);
        await dbContext.NotificationAcknowledgements
            .Where(x => x.AcknowledgedAt < oldestAllowed)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<OutboxMessage>()
            .Where(x => x.SentTime < oldestAllowed)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<OutboxState>()
            .Where(x => x.Created < oldestAllowed)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<InboxState>()
            .Where(x => x.Received < oldestAllowed)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
