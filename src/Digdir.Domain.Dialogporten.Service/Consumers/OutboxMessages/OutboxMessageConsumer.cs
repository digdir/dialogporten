using Digdir.Domain.Dialogporten.Application.Features.V1.Outboxes.Commands.Delete;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;
using MediatR;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Service.Consumers.OutboxMessages;

public sealed class OutboxMessageConsumer : IConsumer<OutboxMessage>
{
    private readonly IMediator _mediatr;

    public OutboxMessageConsumer(IMediator mediatr)
    {
        _mediatr = mediatr ?? throw new ArgumentNullException(nameof(mediatr));
    }

    public async Task Consume(ConsumeContext<OutboxMessage> context)
    {
        var eventAssembly = typeof(OutboxMessage).Assembly;
        var eventType = eventAssembly.GetType(context.Message.EventType);
        var @event = JsonSerializer.Deserialize(context.Message.EventPayload, eventType!); // TODO: Better null handling
        await _mediatr.Publish(@event!); // TODO: Better null handling
        await _mediatr.Send(new DeleteOutboxMessagesCommand { EventId = context.Message.EventId });
    }
}
