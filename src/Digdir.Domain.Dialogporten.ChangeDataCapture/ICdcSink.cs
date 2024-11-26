using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture;

public interface ICdcSink<in T>
{
    Task Send(IReadOnlyCollection<T> outboxMessage, CancellationToken cancellationToken);
}

internal sealed class ConsoleSink : ICdcSink<OutboxMessage>
{
    private readonly ILogger<ConsoleSink> _logger;

    public ConsoleSink(ILogger<ConsoleSink> logger)
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
        // TODO: https://github.com/digdir/dialogporten/issues/...
        // Configure uri
        // TODO: https://github.com/digdir/dialogporten/issues/...
        // Configure transaction
        var endpoint = await _sender.GetSendEndpoint(new Uri("exchange:Digdir.Domain.Dialogporten.Service"));
        await endpoint.SendBatch(
            outboxMessages,
            cancellationToken);
    }
}
