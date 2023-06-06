using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Outboxes.Commands.Delete;

public class DeleteOutboxMessagesCommand : IRequest<OneOf<Success, EntityNotFound>>
{
    public required Guid EventId { get; init; }
}

internal sealed class DeleteOutboxMessagesCommandHandler : IRequestHandler<DeleteOutboxMessagesCommand, OneOf<Success, EntityNotFound>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDialogDbContext _db;

    public DeleteOutboxMessagesCommandHandler(IUnitOfWork unitOfWork, IDialogDbContext db)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<OneOf<Success, EntityNotFound>> Handle(DeleteOutboxMessagesCommand request, CancellationToken cancellationToken)
    {
        var outboxMessage = await _db.OutboxMessages.FindAsync(request.EventId, cancellationToken);

        if (outboxMessage is null)
        {
            return new EntityNotFound<OutboxMessage>(request.EventId);
        }

        _db.OutboxMessages.Remove(outboxMessage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
