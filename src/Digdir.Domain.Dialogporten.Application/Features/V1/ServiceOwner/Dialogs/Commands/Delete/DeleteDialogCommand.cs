using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.EntityFrameworkCore.Features.SoftDeletable;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Delete;

public sealed class DeleteDialogCommand : IRequest<DeleteDialogResult>
{
    public Guid Id { get; set; }
    public Guid? ETag { get; set; }
}

[GenerateOneOf]
public partial class DeleteDialogResult : OneOfBase<Success, EntityNotFound, ConcurrencyError> { }

internal sealed class DeleteDialogCommandHandler : IRequestHandler<DeleteDialogCommand, DeleteDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDialogCommandHandler(IDialogDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<DeleteDialogResult> Handle(DeleteDialogCommand request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .Include(x => x.Elements)
            .Include(x => x.ApiActions)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        _db.TrySetOriginalETag(dialog, request.ETag);
        _db.Dialogs.SoftRemove(dialog);
        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<DeleteDialogResult>(
            success => success,
            domainError => throw new UnreachableException("Should never get a domain error when creating a new dialog"),
            concurrencyError => concurrencyError);
    }
}
