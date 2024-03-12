using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Outboxes.Commands.Delete;

public class DeleteOutboxMessagesCommand : IRequest<DeleteOutboxMessagesResult>
{
    public required Guid EventId { get; init; }
}

[GenerateOneOf]
public partial class DeleteOutboxMessagesResult : OneOfBase<Success, EntityNotFound>;

internal sealed class DeleteOutboxMessagesCommandHandler : IRequestHandler<DeleteOutboxMessagesCommand, DeleteOutboxMessagesResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDialogDbContext _db;

    public DeleteOutboxMessagesCommandHandler(IUnitOfWork unitOfWork, IDialogDbContext db)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<DeleteOutboxMessagesResult> Handle(DeleteOutboxMessagesCommand request, CancellationToken cancellationToken)
    {
        var outboxMessage = await _db.OutboxMessages.FindAsync([request.EventId], cancellationToken: cancellationToken);

        if (outboxMessage is null)
        {
            return new EntityNotFound<OutboxMessage>(request.EventId);
        }

        _db.OutboxMessages.Remove(outboxMessage);
        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        saveResult.Switch(
            success => { },
            domainError => throw new UnreachableException("Should never get a domain error when deleting an outbox message"),
            concurrencyError => throw new UnreachableException("Should never get a concurrency error when deleting an outbox message"));

        return new Success();
    }
}
