using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;

public sealed class OutboxScheduler : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxScheduler> _logger;

    public OutboxScheduler(IServiceScopeFactory scopeFactory, ILogger<OutboxScheduler> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO:
        // As the code stands now its important that there is only one scheduler running at any given time.
        // This code should preferably be processed in another presentation layer, away from the API. If we would
        // like to scale the outbox message processing up in the future we would need to look into tech 
        // solving the concurrency issues here. Alternatively we could implement leader election pattern, or 
        // implement a lock so that there is a 1:1 relationship between an OutboxMessage and a OutboxDispatcher at
        // any given time.
        // 
        // Regardless, this code is sutable for local development without docker. 

        try
        {
            while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) { }

        _logger.LogInformation($"Outbox processing is shuting down due to cancel request.");
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await scope.ServiceProvider
                .GetRequiredService<OutboxDispatcher>()
                .Execute(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Outbox background processing failed.");
        }
    }
}
