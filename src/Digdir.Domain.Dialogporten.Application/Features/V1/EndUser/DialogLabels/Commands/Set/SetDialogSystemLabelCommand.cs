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

public sealed class SetDialogSystemLabelCommand : IRequest<SetDialogSystemLabelResult>
{
    public Guid DialogId { get; set; }
    public SystemLabel.Values Label { get; set; }
}

[GenerateOneOf]
public sealed partial class SetDialogSystemLabelResult : OneOfBase<Success, EntityNotFound, Forbidden, EntityDeleted, DomainError, ConcurrencyError>;

internal sealed class SetDialogSystemLabelHandler : IRequestHandler<SetDialogSystemLabelCommand, SetDialogSystemLabelResult>
{

    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IUserRegistry _userRegistry;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IDialogTokenGenerator _dialogTokenGenerator;

    public SetDialogSystemLabelHandler(IDialogDbContext db, IMapper mapper, IUnitOfWork unitOfWork, IClock clock, IUserRegistry userRegistry, IAltinnAuthorization altinnAuthorization, IDialogTokenGenerator dialogTokenGenerator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _dialogTokenGenerator = dialogTokenGenerator ?? throw new ArgumentNullException(nameof(dialogTokenGenerator));
    }
    public async Task<SetDialogSystemLabelResult> Handle(
        SetDialogSystemLabelCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);
        var dialog = await _db.Dialogs.Include(x => x.DialogEndUserContext).FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken: cancellationToken);
        // var dialogEndUserContext = await _db.DialogEndUserContexts.Include(x => x.Dialog).FirstOrDefaultAsync()

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
        dialog.DialogEndUserContext.UpdateLabel(request.Label, currentUserInformation.UserId.ExternalIdWithPrefix, currentUserInformation.Name);

        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<SetDialogSystemLabelResult>(
            success => success,
            domainError => domainError,
            concurrencyError => concurrencyError);
    }
}
