using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture;

public interface ICdcSink<T>
{
    Task Send(T outboxMessage, CancellationToken cancellationToken);
}

internal sealed class MassTransitSink : ICdcSink<OutboxMessage>
{
    private readonly ISendEndpointProvider _sender;

    public MassTransitSink(ISendEndpointProvider sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task Send(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        // TODO: Configure uri
        var endpoint = await _sender.GetSendEndpoint(new Uri("exchange:Digdir.Domain.Dialogporten.Service"));
        await endpoint.Send(
            outboxMessage,
            cancellationToken);
    }
}
