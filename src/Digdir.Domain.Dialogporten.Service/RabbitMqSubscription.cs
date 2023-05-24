using Digdir.Domain.Dialogporten.Domain.Outboxes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Digdir.Domain.Dialogporten.Service;

public sealed class RabbitMqSubscription
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<RabbitMqSubscription> _logger;
    private readonly IModel _channel;
    private readonly AsyncEventingBasicConsumer _consumer;
    private readonly Channel<RabbitMqMessage> _eventqueue;

    public RabbitMqSubscription(
        IModel channel,
        IHostApplicationLifetime applicationLifetime,
        ILogger<RabbitMqSubscription> logger)
    {
        _channel = channel;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _consumer = new AsyncEventingBasicConsumer(_channel);
        _eventqueue = Channel.CreateUnbounded<RabbitMqMessage>(new()
        {
            SingleWriter= true,
        });
    }

    public async IAsyncEnumerable<RabbitMqMessage> Subscribe([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Register subscription to Rabbit MQ
        _channel.ModelShutdown += ForceShutdown;
        _consumer.Shutdown += ForceShutdownAsync;
        _consumer.Received += ReceivedEvent;
        string consumerTag = _channel.BasicConsume("OutboxQueue", false, _consumer);

        try
        {
            await foreach (var message in _eventqueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    yield return message;
                }
                finally
                {
                    await message.DisposeAsync();
                }
            }
        }
        finally
        {
            // Tear down the Rabbit MQ subscription 
            _channel.ModelShutdown -= ForceShutdown;
            _consumer.Shutdown -= ForceShutdownAsync;
            _consumer.Received -= ReceivedEvent;
            _channel.BasicCancel(consumerTag);
        }
    }

    private async Task ReceivedEvent(object sender, BasicDeliverEventArgs eventArgs)
    {
        await _eventqueue.Writer.WriteAsync(new(_channel, eventArgs));
    }

    private Task ForceShutdownAsync(object? sender, ShutdownEventArgs e)
    {
        ForceShutdown(sender, e);
        return Task.CompletedTask;
    }

    private void ForceShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogCritical("CRITICAL ERROR - RabbitMq connection down. Shutting down application.");
        _applicationLifetime.StopApplication();
    }

    public sealed class RabbitMqMessage : IAsyncDisposable
    {
        private const int MaxNumberOfAttempts = 10;
        private readonly IModel _channel;
        private readonly ulong _deliveryTag;
        private readonly long _deliveryCount;
        
        public object DomainEvent { get; private set; }
        public bool ConfirmationSent { get; private set; }

        public RabbitMqMessage(IModel channel, BasicDeliverEventArgs delivery)
        {
            _channel = channel;
            _deliveryTag = delivery.DeliveryTag;
            _deliveryCount = delivery.BasicProperties.Headers
                .TryGetValue("x-delivery-count", out var count)
                && count is long countAsLong
                ? countAsLong
                : 0;
            DomainEvent = GetDomainEvent(delivery);
        }

        public Task Ack(CancellationToken cancellationToken)
        {
            if (!ConfirmationSent)
            {
                _channel.BasicAck(_deliveryTag, false);
                ConfirmationSent = true;
            }

            return Task.CompletedTask;
        }

        public Task Nack(CancellationToken cancellationToken)
        {
            if (!ConfirmationSent)
            {
                var returnToQueue = _deliveryCount + 1 < MaxNumberOfAttempts;
                _channel.BasicNack(_deliveryTag, false, returnToQueue);
                ConfirmationSent = true;
            }

            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Nack(default));
        }

        private object GetDomainEvent(BasicDeliverEventArgs delivery)
        {
            var eventType = GetEventType(delivery);
            var eventPayload = GetEventPayload(delivery);
            var @event = JsonSerializer.Deserialize(eventPayload, eventType)!;
            return @event;
        }

        private static string GetEventPayload(BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            var messageBodyBytes = Encoding.UTF8.GetString(body);
            return messageBodyBytes;
        }

        private static Type GetEventType(BasicDeliverEventArgs eventArgs)
        {
            if (!eventArgs.BasicProperties.Headers.TryGetValue("x-EventType", out var eventTypeAsObject)
                || eventTypeAsObject is not byte[] eventTypeAsBytes)
            {
                // Expected header not found
                throw new Exception();
            }

            var eventTypeAsString = Encoding.UTF8.GetString(eventTypeAsBytes);
            var eventType = typeof(OutboxMessage).Assembly.GetType(eventTypeAsString);
            return eventType!;
        }
    }
}
