using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Events;

internal class DialogueDeletedDomainEventHandler : INotificationHandler<DialogueDeletedDomainEvent>
{
    public Task Handle(DialogueDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: implement eventhandler
        Console.WriteLine($"Dialogue {notification.DialogueId} deleted.");
        return Task.CompletedTask;
    }
}