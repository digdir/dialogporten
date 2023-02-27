using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Delete;

public sealed class DeleteDialogueCommand : IRequest
{
    public Guid Id { get; set; }
}

internal sealed class DeleteDialogueCommandHandler : IRequestHandler<DeleteDialogueCommand>
{
    private readonly IDialogueDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDialogueCommandHandler(IDialogueDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(DeleteDialogueCommand request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialogue is null) 
        {
            // TODO: Handle with specific exception OR result object
            throw new Exception($"Dialogue with id {request.Id} not found.");
        }

        // TODO: Delete localization sets

        _db.Dialogues.Remove(dialogue);
        // TODO: Publish event
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
