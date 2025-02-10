﻿using System.Diagnostics;
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
using static Digdir.Domain.Dialogporten.Application.Features.V1.Common.Authorization.Constants;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

public sealed class GetDialogQuery : IRequest<GetDialogResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public sealed partial class GetDialogResult : OneOfBase<DialogDto, EntityNotFound, EntityDeleted, Forbidden>;

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, GetDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IUserRegistry _userRegistry;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IDialogTokenGenerator _dialogTokenGenerator;

    public GetDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IClock clock,
        IUserRegistry userRegistry,
        IAltinnAuthorization altinnAuthorization,
        IDialogTokenGenerator dialogTokenGenerator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _dialogTokenGenerator = dialogTokenGenerator ?? throw new ArgumentNullException(nameof(dialogTokenGenerator));
    }

    public async Task<GetDialogResult> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        // This query could be written without all the includes as ProjectTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggregate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behaviours in an expected manner. Therefore, we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Content.OrderBy(x => x.Id).ThenBy(x => x.CreatedAt))
                .ThenInclude(x => x.Value.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.Attachments.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.Attachments.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.GuiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Title!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.GuiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x!.Prompt!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.ApiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Endpoints.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Content)
                .ThenInclude(x => x.Value.Localizations)
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Sender)
                .ThenInclude(x => x.ActorNameEntity)
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Attachments)
                .ThenInclude(x => x.Urls)
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Attachments)
                .ThenInclude(x => x.DisplayName!.Localizations)
            .Include(x => x.Activities)
                .ThenInclude(x => x.Description!.Localizations)
            .Include(x => x.Activities)
                .ThenInclude(x => x.PerformedBy)
                .ThenInclude(x => x.ActorNameEntity)
            .Include(x => x.SeenLog
                    .Where(x => x.CreatedAt >= x.Dialog.UpdatedAt)
                    .OrderBy(x => x.CreatedAt))
                .ThenInclude(x => x.SeenBy)
                    .ThenInclude(x => x.ActorNameEntity)
            .Include(x => x.DialogEndUserContext)
            .Where(x => !x.VisibleFrom.HasValue || x.VisibleFrom < _clock.UtcNowOffset)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken: cancellationToken);

        if (!authorizationResult.HasAccessToMainResource())
        {
            // If the user for some reason does not have access to the main resource, which might be
            // because they are granted access to XACML-actions besides "read" not explicitly defined in the dialog,
            // we do a recheck if the user has access to the dialog via the list authorization. If this is the case,
            // we return the dialog and let DecorateWithAuthorization flag the actions as unauthorized. Note that
            // there might be transmissions that the user has access to, even though there are no authorized actions.
            var listAuthorizationResult = await _altinnAuthorization.HasListAuthorizationForDialog(
                dialog,
                cancellationToken: cancellationToken);

            if (!listAuthorizationResult)
            {
                return new EntityNotFound<DialogEntity>(request.DialogId);
            }
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        // TODO: What if name lookup fails
        // https://github.com/altinn/dialogporten/issues/387
        var currentUserInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);
        dialog.UpdateSeenAt(
            currentUserInformation.UserId.ExternalIdWithPrefix,
            currentUserInformation.UserId.Type,
            currentUserInformation.Name);

        var saveResult = await _unitOfWork
            .DisableUpdatableFilter()
            .DisableVersionableFilter()
            .SaveChangesAsync(cancellationToken);

        saveResult.Switch(
            success => { },
            domainError => throw new UnreachableException("Should not get domain error when updating SeenAt."),
            concurrencyError => throw new UnreachableException("Should not get concurrencyError when updating SeenAt."));

        var dialogDto = _mapper.Map<DialogDto>(dialog);

        dialogDto.SeenSinceLastUpdate = dialog.SeenLog
            .Select(log =>
            {
                var actorId = log.SeenBy.ActorNameEntity != null ? log.SeenBy.ActorNameEntity.ActorId : log.SeenBy.ActorId;
                var logDto = _mapper.Map<DialogSeenLogDto>(log);
                logDto.IsCurrentEndUser = currentUserInformation.UserId.ExternalIdWithPrefix == actorId;
                return logDto;
            })
            .ToList();

        dialogDto.DialogToken = _dialogTokenGenerator.GetDialogToken(
            dialog,
            authorizationResult,
            DialogTokenIssuerVersion
        );

        DecorateWithAuthorization(dialogDto, authorizationResult);
        ReplaceUnauthorizedUrls(dialogDto);

        return dialogDto;
    }

    private static void DecorateWithAuthorization(DialogDto dto,
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

            var authorizedTransmissions = dto.Transmissions.Where(t => authorizationResult.HasReadAccessToDialogTransmission(t.AuthorizationAttribute));
            foreach (var transmission in authorizedTransmissions)
            {
                transmission.IsAuthorized = true;
            }
        }
    }

    private static void ReplaceUnauthorizedUrls(DialogDto dto)
    {
        // For all API and GUI actions and transmissions where isAuthorized is false, replace the URLs with Constants.UnauthorizedUrl
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

        foreach (var dialogTransmission in dto.Transmissions.Where(e => !e.IsAuthorized))
        {
            var urls = dialogTransmission.Attachments.SelectMany(a => a.Urls).ToList();
            foreach (var url in urls)
            {
                url.Url = Constants.UnauthorizedUri;
            }
        }
    }
}
