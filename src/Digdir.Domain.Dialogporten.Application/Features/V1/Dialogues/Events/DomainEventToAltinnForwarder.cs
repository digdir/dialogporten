using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Events;

internal sealed class DomainEventToAltinnForwarder :
    INotificationHandler<DialogueCreatedDomainEvent>,
    INotificationHandler<DialogueUpdatedDomainEvent>,
    INotificationHandler<DialogueDeletedDomainEvent>
{
    public Task Handle(DialogueCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} created.");
        return Task.CompletedTask;
    }

    public Task Handle(DialogueUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} updated.");
        return Task.CompletedTask;
    }

    public Task Handle(DialogueDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Dialogue {notification.DialogueId} deleted.");
        return Task.CompletedTask;
    }
}
