using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Digdir.Domain.Dialogporten.Service;

public sealed class RabbitMqConnection
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConnection()
    {
        _connectionFactory = new()
        {
            ClientProvidedName = "Digdir.Domain.Dialogporten.Service",
            DispatchConsumersAsync = true,
        };
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }

}



public interface IEventBus
{
    void Publish(IDomainEvent @event);

    void Subscribe<TDomainEvent, TDomainEventHandler>()
        where TDomainEvent : IDomainEvent
        where TDomainEventHandler : IDomainEventHandler<TDomainEvent>;

    void Unsubscribe<TDomainEvent, TDomainEventHandler>()
        where TDomainEventHandler : IDomainEventHandler<TDomainEvent>
        where TDomainEvent : IDomainEvent;
}

public sealed class RabbitMQEventBus : IEventBus
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly IConnection _connection;
    private readonly IModel _rabbitChannel;
    private readonly AsyncEventingBasicConsumer _consumer;
    private readonly Channel<RabbitMqMessage> _messageChannel;

    //private readonly AsyncEventingBasicConsumer _consumer;

    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public RabbitMQEventBus(ILogger<RabbitMQEventBus> logger, IHostApplicationLifetime applicationLifetime)
    {
        // TODO: Add connection configuration
        _connectionFactory = new()
        {
            ClientProvidedName = "Digdir.Domain.Dialogporten.Service",
            DispatchConsumersAsync = true,
        };
        _connection = _connectionFactory.CreateConnection();
        _rabbitChannel = _connection.CreateModel();

        _consumer = new AsyncEventingBasicConsumer(_rabbitChannel);
        _messageChannel = Channel.CreateUnbounded<RabbitMqMessage>(new()
        {
            SingleWriter = true,
        });

        _connection.ConnectionShutdown += ForceShutdown;
        _rabbitChannel.ModelShutdown += ForceShutdown;
        _consumer.Shutdown += ForceShutdownAsync;
        _consumer.Received += ReceivedEvent;
        
        _logger = logger;
        _applicationLifetime = applicationLifetime;

        /* How do we adjust the degree of paralellism?
         * 
         */
    }

    private async Task ReceivedEvent(object sender, BasicDeliverEventArgs eventArgs)
    {
        var msg = await RabbitMqMessage.Create(_rabbitChannel, eventArgs);
        await _messageChannel.Writer.WriteAsync(msg);
    }

    public void Publish(IDomainEvent @event)
    {
        throw new NotImplementedException();
    }

    public void Subscribe<TDomainEvent, TDomainEventHandler>()
        where TDomainEvent : IDomainEvent
        where TDomainEventHandler : IDomainEventHandler<TDomainEvent>
    {
        throw new NotImplementedException();
    }

    public void Unsubscribe<TDomainEvent, TDomainEventHandler>()
        where TDomainEvent : IDomainEvent
        where TDomainEventHandler : IDomainEventHandler<TDomainEvent>
    {
        throw new NotImplementedException();
    }

    private void ForceShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogCritical("CRITICAL ERROR - RabbitMq connection down. Shutting down application.");
        _applicationLifetime.StopApplication();
    }

    private Task ForceShutdownAsync(object? sender, ShutdownEventArgs e)
    {
        ForceShutdown(sender, e);
        return Task.CompletedTask;
    }
}

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Hande(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public class DomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly IPublisher _mediatr;

    public DomainEventHandler(IPublisher mediatr)
    {
        _mediatr = mediatr;
    }

    public Task Hande(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _mediatr.Publish(domainEvent, cancellationToken);
    }
}

internal sealed class RabbitMqMessage
{
    private const int MaxNumberOfAttempts = 10;
    private readonly IModel _channel;
    private readonly ulong _deliveryTag;
    private readonly long _deliveryCount;
    
    private bool _confirmationSent;

    public IDomainEvent DomainEvent { get; }

    public static Task<RabbitMqMessage> Create(IModel channel, BasicDeliverEventArgs delivery)
    {
        var deliveryCount = delivery.BasicProperties.Headers
            .TryGetValue("x-delivery-count", out var count)
            && count is long countAsLong
            ? countAsLong
            : 0;
        var domainEvent = GetDomainEvent(delivery);
        return Task.FromResult(new RabbitMqMessage(channel, delivery.DeliveryTag, deliveryCount, domainEvent));
    }

    private RabbitMqMessage(
        IModel channel, 
        ulong deliveryTag,
        long deliveryCount,
        IDomainEvent domainEvent)
    {
        _channel = channel;
        _deliveryTag = deliveryTag;
        _deliveryCount = deliveryCount;
        DomainEvent = domainEvent;
    }

    public Task Ack(CancellationToken cancellationToken)
    {
        if (!_confirmationSent)
        {
            _channel.BasicAck(_deliveryTag, false);
            _confirmationSent = true;
        }

        return Task.CompletedTask;
    }

    public Task Nack(CancellationToken cancellationToken)
    {
        if (!_confirmationSent)
        {
            var returnToQueue = _deliveryCount + 1 < MaxNumberOfAttempts;
            _channel.BasicNack(_deliveryTag, false, returnToQueue);
            _confirmationSent = true;
        }

        return Task.CompletedTask;
    }

    private static IDomainEvent GetDomainEvent(BasicDeliverEventArgs delivery)
    {
        var eventType = GetEventType(delivery);
        var eventPayload = GetEventPayload(delivery);
        var @event = (IDomainEvent) JsonSerializer.Deserialize(eventPayload, eventType)!;
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

        if (eventType is null)
        {
            throw new Exception();
        }

        if (!eventType.IsAssignableTo(typeof(IDomainEvent)))
        {
            throw new Exception();
        }

        return eventType;
    }
}