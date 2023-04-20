using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Events;

internal class DialogueUpdatedDomainEventHandler : INotificationHandler<DialogueUpdatedDomainEvent>
{
    public Task Handle(DialogueUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: implement eventhandler
        Console.WriteLine($"Dialogue {notification.DialogueId} updated.");
        return Task.CompletedTask;
    }
}
