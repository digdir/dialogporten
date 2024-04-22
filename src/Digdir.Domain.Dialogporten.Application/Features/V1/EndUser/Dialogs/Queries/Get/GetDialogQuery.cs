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
public partial class GetDialogResult : OneOfBase<GetDialogDto, EntityNotFound, EntityDeleted, Forbidden>;

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, GetDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IUserNameRegistry _userNameRegistry;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IDialogTokenGenerator _dialogTokenGenerator;
    private readonly IStringHasher _stringHasher;

    public GetDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IClock clock,
        IUserNameRegistry userNameRegistry,
        IAltinnAuthorization altinnAuthorization,
        IDialogTokenGenerator dialogTokenGenerator,
        IStringHasher stringHasher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userNameRegistry = userNameRegistry ?? throw new ArgumentNullException(nameof(userNameRegistry));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _dialogTokenGenerator = dialogTokenGenerator ?? throw new ArgumentNullException(nameof(dialogTokenGenerator));
        _stringHasher = stringHasher;
    }

    public async Task<GetDialogResult> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        var userInformation = await _userNameRegistry.GetUserInformation(cancellationToken);

        if (userInformation is null)
        {
            return new Forbidden("No valid user pid found.");
        }

        var (userPid, userName) = userInformation;

        // This query could be written without all the includes as ProjectTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggregate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behaviours in an expected manner. Therefore, we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Content.OrderBy(x => x.Id).ThenBy(x => x.CreatedAt))
                .ThenInclude(x => x.Value.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.GuiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Title!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.ApiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Endpoints.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.Activities).ThenInclude(x => x.PerformedBy!.Localizations)
            .Include(x => x.Activities).ThenInclude(x => x.Description!.Localizations)
            .Include(x => x.SeenLog
                .Where(x => x.CreatedAt >= x.Dialog.UpdatedAt)
                .OrderBy(x => x.CreatedAt))
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

        // TODO: What if name lookup fails
        // https://github.com/digdir/dialogporten/issues/387
        dialog.UpdateSeenAt(userPid, userName);

        var saveResult = await _unitOfWork
            .WithoutAuditableSideEffects()
            .SaveChangesAsync(cancellationToken);

        saveResult.Switch(
            success => { },
            domainError => throw new UnreachableException("Should not get domain error when updating SeenAt."),
            concurrencyError => throw new UnreachableException("Should not get concurrencyError when updating SeenAt."));

        var dialogDto = _mapper.Map<GetDialogDto>(dialog);

        dialogDto.SeenSinceLastUpdate = dialog.SeenLog
            .Select(log =>
            {
                var logDto = _mapper.Map<GetDialogDialogSeenLogDto>(log);
                logDto.IsCurrentEndUser = log.EndUserId == userPid;
                logDto.EndUserIdHash = _stringHasher.Hash(log.EndUserId);
                return logDto;
            })
            .ToList();

        dialogDto.DialogToken = _dialogTokenGenerator.GetDialogToken(
            dialog,
            authorizationResult,
            "api/v1"
        );

        DecorateWithAuthorization(dialogDto, authorizationResult);
        ReplaceUnauthorizedUrls(dialogDto);

        return dialogDto;
    }

    private static void DecorateWithAuthorization(GetDialogDto dto,
        DialogDetailsAuthorizationResult authorizationResult)
    {
        foreach (var (action, resource) in authorizationResult.AuthorizedAltinnActions)
        {
            foreach (var apiAction in dto.ApiActions.Where(a => a.Action == action))
            {
                if ((apiAction.AuthorizationAttribute is null && resource == Constants.MainResource)
                 || (apiAction.AuthorizationAttribute is not null && resource == apiAction.AuthorizationAttribute))
                {
                    apiAction.IsAuthorized = true;
                }
            }

            foreach (var guiAction in dto.GuiActions.Where(a => a.Action == action))
            {
                if ((guiAction.AuthorizationAttribute is null && resource == Constants.MainResource)
                 || (guiAction.AuthorizationAttribute is not null && resource == guiAction.AuthorizationAttribute))
                {
                    guiAction.IsAuthorized = true;
                }
            }

            // Simple "read" on the main resource will give access to a dialog element, unless an authorization attribute is set,
            // in which case an "elementread" action is required
            var elements = dto.Elements.Where(dialogElement =>
                (dialogElement.AuthorizationAttribute is null && action == Constants.ReadAction) ||
                (dialogElement.AuthorizationAttribute is not null && action == Constants.ElementReadAction));

            foreach (var dialogElement in elements)
            {
                dialogElement.IsAuthorized = true;
            }
        }
    }

    private static void ReplaceUnauthorizedUrls(GetDialogDto dto)
    {
        // For all API and GUI actions and dialogelements where isAuthorized is false, replace the URLs with Constants.UnauthorizedUrl
        foreach (var guiAction in dto.GuiActions.Where(a => !a.IsAuthorized))
        {
            guiAction.Url = Constants.UnauthorizedUri;
        }

        foreach (var apiAction in dto.ApiActions.Where(a => !a.IsAuthorized))
        {
            foreach (var endpoint in apiAction.Endpoints)
            {
                endpoint.Url = Constants.UnauthorizedUri;
            }
        }

        foreach (var dialogElement in dto.Elements.Where(e => !e.IsAuthorized))
        {
            foreach (var url in dialogElement.Urls)
            {
                url.Url = Constants.UnauthorizedUri;
            }
        }
    }
}
