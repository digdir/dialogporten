using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Delete;

public sealed class DeleteDialogueCommand : IRequest<OneOf<Success, NotFound>>
{
    public Guid Id { get; set; }
}

internal sealed class DeleteDialogueCommandHandler : IRequestHandler<DeleteDialogueCommand, OneOf<Success, NotFound>>
{
    private readonly IDialogueDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDialogueCommandHandler(IDialogueDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<OneOf<Success, NotFound>> Handle(DeleteDialogueCommand request, CancellationToken cancellationToken)
    {
        var dialogue = await _db.Dialogues
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialogue is null) 
        {
            return new NotFound();
            //throw new Exception($"Dialogue with id {request.Id} not found.");
        }

        // TODO: Delete localization sets

        _db.Dialogues.Remove(dialogue);
        // TODO: Publish event
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new Success();
    }
}
