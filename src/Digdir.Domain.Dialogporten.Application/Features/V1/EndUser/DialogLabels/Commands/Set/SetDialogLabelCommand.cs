using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Actors;
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
public sealed partial class SetDialogLabelResult : OneOfBase<Success, EntityNotFound, Forbidden, EntityDeleted, DomainError, ValidationError, ConcurrencyError>;

internal sealed class SetDialogLabelHandler : IRequestHandler<SetDialogLabelCommand, SetDialogLabelResult>
{

    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IUserRegistry _userRegistry;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IDialogTokenGenerator _dialogTokenGenerator;

    public SetDialogLabelHandler(IDialogDbContext db, IMapper mapper, IUnitOfWork unitOfWork, IClock clock, IUserRegistry userRegistry, IAltinnAuthorization altinnAuthorization, IDialogTokenGenerator dialogTokenGenerator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _dialogTokenGenerator = dialogTokenGenerator ?? throw new ArgumentNullException(nameof(dialogTokenGenerator));
    }
    public async Task<SetDialogLabelResult> Handle(
        SetDialogLabelCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);
        var dialog = await _db.Dialogs.Include(x => x.DialogEndUserContext).FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(dialog, cancellationToken: cancellationToken);
        if (!authorizationResult.HasAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }
        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        // TEMP, set default to new() in migration
        dialog.DialogEndUserContext ??= new();

        // Amund: Her skal ting gjæres, dialog e ferdig fonne, endUserContext e joina, bruker har accessToMainResource!
        // Nå kan SystemLabel oppdateres?!
        // Amund: dette føles for manulet ut det er sikkert noe magi jeg kan ta i bruk her det blir ikke å fungere på default heller her må noe annet gjøres 
        // det funker om default også har prefixen. Validator har sjekket at dette skal funke kan dette flyttes inn i mapper? mappe til namespace og label i mapper? det virker vartfall bedre plassert enn å ha det er her
        _ = Enum.TryParse(request.Label.Split(":")[1], true, out SystemLabel.Values labelId);
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
