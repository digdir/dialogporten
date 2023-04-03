using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Delete;

public sealed class DeleteDialogueCommand : IRequest<OneOf<Success, EntityNotFound>>
{
    public Guid Id { get; set; }
}

internal sealed class DeleteDialogueCommandHandler : IRequestHandler<DeleteDialogueCommand, OneOf<Success, EntityNotFound>>
{
    private readonly IDialogueDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _eventPublisher;

    public DeleteDialogueCommandHandler(IDialogueDbContext db, IUnitOfWork unitOfWork, IDomainEventPublisher eventPublisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<OneOf<Success, EntityNotFound>> Handle(DeleteDialogueCommand request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialogue is null) 
        {
            return new EntityNotFound<DialogueEntity>(request.Id);
        }

        // TODO: Delete localization sets

        _db.Dialogues.Remove(dialogue);
        _eventPublisher.Publish(new DialogueDeletedDomainEvent(dialogue.Id));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new Success();
    }
}
