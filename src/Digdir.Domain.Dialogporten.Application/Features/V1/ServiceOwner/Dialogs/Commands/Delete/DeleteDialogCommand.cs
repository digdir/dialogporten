using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
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
    public Guid? IfMatchDialogRevision { get; set; }
}

[GenerateOneOf]
public sealed partial class DeleteDialogResult : OneOfBase<Success, EntityNotFound, BadRequest, Forbidden, ConcurrencyError>;

internal sealed class DeleteDialogCommandHandler : IRequestHandler<DeleteDialogCommand, DeleteDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserResourceRegistry _userResourceRegistry;

    public DeleteDialogCommandHandler(
        IDialogDbContext db,
        IUnitOfWork unitOfWork,
        IUserResourceRegistry userResourceRegistry)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    }

    public async Task<DeleteDialogResult> Handle(DeleteDialogCommand request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userResourceRegistry.GetCurrentUserResourceIds(cancellationToken);

        var dialog = await _db.Dialogs
            .Include(x => x.Activities)
            .WhereIf(!_userResourceRegistry.IsCurrentUserServiceOwnerAdmin(), x => resourceIds.Contains(x.ServiceResource))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        if (dialog.Deleted)
        {
            // TODO: https://github.com/digdir/dialogporten/issues/1543
            // When restoration is implemented, add a hint to the error message.
            return new BadRequest($"Entity '{nameof(DialogEntity)}' with key '{request.Id}' is already removed, and cannot be deleted again.");
        }

        if (!_userResourceRegistry.UserCanModifyResourceType(dialog.ServiceResourceType))
        {
            return new Forbidden($"User cannot modify resource type {dialog.ServiceResourceType}.");
        }

        _db.Dialogs.SoftRemove(dialog);
        var saveResult = await _unitOfWork
            .EnableConcurrencyCheck(dialog, request.IfMatchDialogRevision)
            .SaveChangesAsync(cancellationToken);

        return saveResult.Match<DeleteDialogResult>(
            success => success,
            domainError => throw new UnreachableException("Should never get a domain error when deleting a dialog"),
            concurrencyError => concurrencyError);
    }
}
