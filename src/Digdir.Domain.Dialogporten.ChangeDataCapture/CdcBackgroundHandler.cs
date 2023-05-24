using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Sinks;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture;

internal sealed class CdcBackgroundHandler : BackgroundService
{
    private readonly IPostgresCdcSubscription _subscription;
    private readonly ISink _sink;
    private readonly ILogger<CdcBackgroundHandler> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public CdcBackgroundHandler(
        IPostgresCdcSubscription subscription,
        ISink sink,
        ILogger<CdcBackgroundHandler> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _sink.Init(stoppingToken);

            await foreach (var @event in _subscription.Subscribe(stoppingToken))
            {
                // TODO: Handle publisher confirmes https://www.rabbitmq.com/confirms.html
                await _sink.Send(@event, stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unhandled exception in CDC background handler. Shutting down application.");
            _applicationLifetime.StopApplication();
        }
    }
}
