using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using MassTransit;
using MediatR;

namespace Digdir.Domain.Dialogporten.Service.Consumers;

public sealed class DialogCreatedDomainEventConsumer(ISender sender, IPublisher publisher) : IConsumer<DialogCreatedDomainEvent>
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task Consume(ConsumeContext<DialogCreatedDomainEvent> context)
    {
        context.
        await _publisher.Publish(context.Message, context.CancellationToken);
    }
}
