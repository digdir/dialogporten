using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscriptions;
using Digdir.Domain.Dialogporten.Domain.Outboxes;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;

internal sealed class OutboxCdcBackgroundHandler : BackgroundService
{
    private readonly ICdcSubscription<OutboxMessage> _subscription;
    private readonly ILogger<OutboxCdcBackgroundHandler> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxCdcBackgroundHandler(
        ICdcSubscription<OutboxMessage> subscription,
        ILogger<OutboxCdcBackgroundHandler> logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory scopeFactory)
    {
        _subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var outboxMessage in _subscription.Subscribe(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var sink = scope.ServiceProvider.GetRequiredService<ICdcSink<OutboxMessage>>();
                await sink.Send(outboxMessage, stoppingToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Environment.ExitCode = -1;
            _logger.LogCritical(ex, "Unhandled exception in CDC background handler. Shutting down application.");
            _applicationLifetime.StopApplication();
        }
    }
}
