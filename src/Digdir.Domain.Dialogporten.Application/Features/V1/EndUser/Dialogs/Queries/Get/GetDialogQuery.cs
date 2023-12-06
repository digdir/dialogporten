using System.Diagnostics;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

public sealed class GetDialogQuery : IRequest<GetDialogResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogResult : OneOfBase<GetDialogDto, EntityNotFound, EntityDeleted> { }

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, GetDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionTime _transactionTime;
    private readonly IClock _clock;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public GetDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ITransactionTime transactionTime,
        IClock clock,
        IAltinnAuthorization altinnAuthorization)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<GetDialogResult> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        // This query could be written without all the includes as ProjectTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggregate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behaviours in an expected manner. Therefore we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Body!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Title!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.SenderName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.GuiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Title!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.ApiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Endpoints.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Where(x => !x.VisibleFrom.HasValue || x.VisibleFrom < _clock.UtcNowOffset)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken);

        if (!authorizationResult.HasReadAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        dialog.UpdateReadAt(_transactionTime.Value);

        var saveResult = await _unitOfWork
            .WithoutAuditableSideEffects()
            .SaveChangesAsync(cancellationToken);

        saveResult.Switch(
            success => { },
            domainError => throw new UnreachableException("Should not get domain error when updating ReadAt."),
            concurrencyError => throw new UnreachableException("Should not get concurrencyError when updating ReadAt."));

        var dto = _mapper.Map<GetDialogDto>(dialog);

        DecorateWithAuthorization(dto, authorizationResult);

        return dto;
    }

    private static void DecorateWithAuthorization(GetDialogDto dto, DialogDetailsAuthorizationResult authorizationResult)
    {
        foreach (var (action, resources) in authorizationResult.AuthorizedAltinnActions)
        {
            foreach (var apiAction in dto.ApiActions.Where(a => a.Action == action))
            {
                if ((apiAction.AuthorizationAttribute is null && resources.Contains(Constants.MainResource))
                    || (apiAction.AuthorizationAttribute is not null && resources.Contains(apiAction.AuthorizationAttribute)))
                {
                    apiAction.IsAuthorized = true;
                }
            }

            foreach (var guiAction in dto.GuiActions.Where(a => a.Action == action))
            {
                if ((guiAction.AuthorizationAttribute is null && resources.Contains(Constants.MainResource))
                    || (guiAction.AuthorizationAttribute is not null && resources.Contains(guiAction.AuthorizationAttribute)))
                {
                    guiAction.IsAuthorized = true;
                }
            }

            // Simple "read" on the main resource will give access to a dialog element, unless a authorization attribute is set,
            // in which case an "elementread" action is required
            foreach (var dialogElement in dto.Elements.Where(
                         dialogElement => (dialogElement.AuthorizationAttribute is null)
                                          || (dialogElement.AuthorizationAttribute is not null
                                              && action == Constants.ElementReadAction)))
            {
                dialogElement.IsAuthorized = true;
            }
        }
    }
}
