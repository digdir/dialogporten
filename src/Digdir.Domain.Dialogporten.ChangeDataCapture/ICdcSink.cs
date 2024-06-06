using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture;

public interface ICdcSink<T>
{
    Task Send(IReadOnlyCollection<T> outboxMessage, CancellationToken cancellationToken);
}

internal sealed class ConcoleSink : ICdcSink<OutboxMessage>
{
    private readonly ILogger<ConcoleSink> _logger;

    public ConcoleSink(ILogger<ConcoleSink> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Send(IReadOnlyCollection<OutboxMessage> outboxMessages, CancellationToken cancellationToken)
    {
        foreach (var outboxMessage in outboxMessages)
        {
            _logger.LogDebug("Sending {EventType} {EventId} to message bus with payload {EventPayload}.",
                outboxMessage.EventType, outboxMessage.EventId, outboxMessage.EventPayload);
        }

        return Task.CompletedTask;
    }
}

internal sealed class MassTransitSink : ICdcSink<OutboxMessage>
{
    private readonly ISendEndpointProvider _sender;

    public MassTransitSink(ISendEndpointProvider sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task Send(IReadOnlyCollection<OutboxMessage> outboxMessages, CancellationToken cancellationToken)
    {
        // TODO: Configure uri
        // TODO: Configure transacton
        var endpoint = await _sender.GetSendEndpoint(new Uri("exchange:Digdir.Domain.Dialogporten.Service"));
        await endpoint.SendBatch(
            outboxMessages,
            cancellationToken);
    }
}
