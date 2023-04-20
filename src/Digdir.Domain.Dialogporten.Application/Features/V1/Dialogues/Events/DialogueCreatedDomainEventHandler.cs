using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Events;

internal class DialogueCreatedDomainEventHandler : INotificationHandler<DialogueCreatedDomainEvent>
{
    public Task Handle(DialogueCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} created.");
        return Task.CompletedTask;
    }
}
