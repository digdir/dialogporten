using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Sinks;

internal sealed class RabbitMqSink : ISink, IDisposable
{
    private const string DeadLetterExchangeName = "OutboxDeadLetterEcchange";
    private const string DeadLetterQueueName = "OutboxDeadLetterQueue";
    private const string ExchangeName = "OutboxEcchange";
    private const string QueueName = "OutboxQueue";
    private const string RoutingKey = "OutboxRoutingKey";

    private readonly ConnectionFactory _factory = new()
    {
        // "guest"/"guest" by default, limited to localhost connections
        //factory.UserName = user;
        //factory.Password = pass;
        //factory.VirtualHost = vhost;
        //factory.HostName = hostName;

        AutomaticRecoveryEnabled = true,
        TopologyRecoveryEnabled = true,
        ClientProvidedName = "Digdir.Domain.Dialogporten.ChangeDataCapture"
    };
    private IConnection? _connection;
    private IModel? _channel;

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }

    public Task Init(CancellationToken cancellationToken)
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Create exchanges
        _channel.ExchangeDeclare(DeadLetterExchangeName, ExchangeType.Direct);
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

        // Create queues
        _channel.QueueDeclare(QueueName, true, false, false, new Dictionary<string, object>
        {
            { "x-queue-type", "quorum" },
            { "x-dead-letter-strategy", "at-least-once" },
            { "x-dead-letter-exchange", DeadLetterExchangeName },
            { "x-overflow", "reject-publish" },
        });
        _channel.QueueDeclare(DeadLetterQueueName, true, false, false, new Dictionary<string, object>
        {
            { "x-queue-type", "quorum" }
        });

        // Bind queues to exchanges
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey, null);
        _channel.QueueBind(DeadLetterQueueName, DeadLetterExchangeName, RoutingKey, null);

        return Task.CompletedTask;
    }

    public Task Send(object @event, CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("The sink has not been initialized.");
        }

        var eventType = @event.GetType().FullName!;
        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object>()
        {
            { "x-EventType", Encoding.UTF8.GetBytes(eventType) }
        };
        var eventAsJson = JsonSerializer.Serialize(@event);
        var messageBodyBytes = Encoding.UTF8.GetBytes(eventAsJson);
        _channel.BasicPublish(ExchangeName, RoutingKey, true, properties, messageBodyBytes);
        return Task.CompletedTask;
    }
}
