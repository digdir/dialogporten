using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSystemLabels.Commands.Set;

public sealed class SetDialogSystemLabelCommand : SetDialogSystemLabelDto, IRequest<SetDialogLabelResult>
{
    public Guid? IfMatchDialogRevision { get; set; }
}

[GenerateOneOf]
public sealed partial class SetDialogLabelResult : OneOfBase<Success, EntityNotFound, EntityDeleted, DomainError, ValidationError, ConcurrencyError>;

internal sealed class SetDialogLabelCommandHandler : IRequestHandler<SetDialogSystemLabelCommand, SetDialogLabelResult>
{
    private readonly IDialogDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRegistry _userRegistry;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public SetDialogLabelCommandHandler(IDialogDbContext db, IUnitOfWork unitOfWork, IUserRegistry userRegistry, IAltinnAuthorization altinnAuthorization)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<SetDialogLabelResult> Handle(
        SetDialogSystemLabelCommand request,
        CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
                              .Include(x => x.DialogEndUserContext)
                              .FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(dialog, cancellationToken: cancellationToken);
        if (!authorizationResult.HasAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var currentUserInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);


        /*
         * Amund:
         *  [x] Rename meste parten til systenLabel istedet for label
         *  [x] Post til put
         *  [x] Fjerne intak av prefix på enduser endpoint
         *  [ ] Search med flere labels? 
         * 
         * POST api/v1/enduser/dialogs/{dialogId}/labels
         * [
         *    "magnusSineLabels:HelloWorld",
         *    "systemlabels:bin",
         * [
         *
         * DELETE api/v1/enduser/dialogs/{dialogId}/labels
         * [
         *    "magnusSineLabels:HelloWorld",
         *    "systemlabels:bin",
         * [
         *
         * PUT api/v1/enduser/dialogs/{dialogId}/systemlabels
          {
         *     "label": "bin"
         * }
         * Er dette bedre?
         * PUT api/v1/enduser/dialogs/{dialogId}/systemlabels/{label}
         */


        dialog.DialogEndUserContext.UpdateLabel(request.Label, currentUserInformation.UserId.ExternalIdWithPrefix, currentUserInformation.Name);

        var saveResult = await _unitOfWork
                               .EnableConcurrencyCheck(dialog, request.IfMatchDialogRevision)
                               .SaveChangesAsync(cancellationToken);
        return saveResult.Match<SetDialogLabelResult>(
            success => success,
            domainError => domainError,
            concurrencyError => concurrencyError);
    }
}
