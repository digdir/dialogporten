using MediatR;

namespace Digdir.Domain.Dialogporten.Service;

internal sealed class RabbitMqSubscriber : BackgroundService
{
    private readonly RabbitMqSubscription _rabbitSubscription;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqSubscriber> _logger;

    public RabbitMqSubscriber(
        RabbitMqSubscription rabbitSubscription,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqSubscriber> logger)
    {
        _rabbitSubscription = rabbitSubscription;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _rabbitSubscription.Subscribe(stoppingToken))
        {
            try
            {
                // TODO: Retry
                using var scope = _scopeFactory.CreateScope();
                await scope.ServiceProvider
                    .GetRequiredService<IPublisher>()
                    .Publish(message.DomainEvent, stoppingToken);
                await message.Ack(stoppingToken);
            }
            catch (Exception)
            {
                _logger.LogError("Unable to process domain event, sending it to dead letter queue.");
                await message.Nack(stoppingToken);
            }
        }
    }
}