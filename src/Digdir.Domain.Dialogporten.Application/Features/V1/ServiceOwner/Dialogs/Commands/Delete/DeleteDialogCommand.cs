using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common;
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
    public Guid? Revision { get; set; }
}

[GenerateOneOf]
public partial class DeleteDialogResult : OneOfBase<Success, EntityNotFound, ConcurrencyError> { }

internal sealed class DeleteDialogCommandHandler : IRequestHandler<DeleteDialogCommand, DeleteDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public DeleteDialogCommandHandler(
        IDialogDbContext db,
        IUnitOfWork unitOfWork,
        IUserService userService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<DeleteDialogResult> Handle(DeleteDialogCommand request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userService.GetCurrentUserResourceIds(cancellationToken);

        var dialog = await _db.Dialogs
            // Load the elements so that we notify them of their deletion. (This won't work due to https://github.com/digdir/dialogporten/issues/288)
            .Include(x => x.Elements)
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        _db.TrySetOriginalRevision(dialog, request.Revision);
        _db.Dialogs.SoftRemove(dialog);
        var saveResult = await _unitOfWork.SaveChangesAsync(optimisticConcurrency: !request.Revision.HasValue, cancellationToken);
        return saveResult.Match<DeleteDialogResult>(
            success => success,
            domainError => throw new UnreachableException("Should never get a domain error when creating a new dialog"),
            concurrencyError => concurrencyError);
    }
}
