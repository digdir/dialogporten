using Digdir.Domain.Dialogporten.Domain;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

internal class DialogueCreatedDomainEventHandler : INotificationHandler<DialogueCreatedDomainEvent>
{
    public Task Handle(DialogueCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Send to message service
        throw new NotImplementedException();
    }
}
