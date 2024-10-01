using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Commands.Set;

public sealed class SetDialogLabelCommand : SetDialogLabelDto, IRequest<SetDialogLabelResult>
{
    public Guid? IfMatchDialogRevision { get; set; }
}

[GenerateOneOf]
public sealed partial class SetDialogLabelResult : OneOfBase<Success, EntityNotFound, EntityDeleted, DomainError, ValidationError, ConcurrencyError>;

internal sealed class SetDialogLabelCommandHandler : IRequestHandler<SetDialogLabelCommand, SetDialogLabelResult>
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
        SetDialogLabelCommand request,
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
         * {
         *     "label": "bin"
         * }
         * 
         * 
         * 
         * 
         * 
         */
        
        // Amund: dette føles for manulet ut det er sikkert noe magi jeg kan ta i bruk her det blir ikke å fungere på default heller her må noe annet gjøres 
        // det funker om default også har prefixen. Validator har sjekket at dette skal
        // funke kan dette flyttes inn i mapper? mappe til namespace og label i mapper?
        // det virker vartfall bedre plassert enn å ha det er her
        var labelId = Enum.Parse<SystemLabel.Values>(
            request.Label.Split(":")[1],
            ignoreCase: true);

        dialog.DialogEndUserContext.UpdateLabel(labelId, currentUserInformation.UserId.ExternalIdWithPrefix, currentUserInformation.Name);

        var saveResult = await _unitOfWork
            .EnableConcurrencyCheck(dialog, request.IfMatchDialogRevision)
            .SaveChangesAsync(cancellationToken);
        return saveResult.Match<SetDialogLabelResult>(
            success => success,
            domainError => domainError,
            concurrencyError => concurrencyError);
    }
}
