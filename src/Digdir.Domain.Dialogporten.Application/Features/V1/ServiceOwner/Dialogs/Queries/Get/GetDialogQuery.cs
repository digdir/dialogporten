﻿using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;

public sealed class GetDialogQuery : IRequest<GetDialogResult>
{
    public Guid DialogId { get; set; }
    // query param
}

[GenerateOneOf]
public partial class GetDialogResult : OneOfBase<GetDialogDto, EntityNotFound>;

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, GetDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUserResourceRegistry _userResourceRegistry;

    public GetDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUserResourceRegistry userResourceRegistry)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    }

    public async Task<GetDialogResult> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userResourceRegistry.GetCurrentUserResourceIds(cancellationToken);

        // This query could be written without all the includes as ProjectTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggregate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behaviours in an expected manner. Therefore we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Content.OrderBy(x => x.Id).ThenBy(x => x.CreatedAt))
                .ThenInclude(x => x.Value.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.SearchTags.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
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
            .Include(x => x.Transmissions).ThenInclude(x => x.Sender)
            .Include(x => x.Transmissions).ThenInclude(x => x.Attachments).ThenInclude(x => x.Urls)
            .Include(x => x.Transmissions).ThenInclude(x => x.Attachments).ThenInclude(x => x.DisplayName!.Localizations)
            .Include(x => x.Activities).ThenInclude(x => x.Description!.Localizations)
            .Include(x => x.Activities).ThenInclude(x => x.PerformedBy)
            .Include(x => x.SeenLog
                .Where(x => x.CreatedAt >= x.Dialog.UpdatedAt)
                .OrderBy(x => x.CreatedAt))
                .ThenInclude(x => x.SeenBy)
            .IgnoreQueryFilters()
            .AsNoTracking() // TODO: Remove when #386 is implemented,
            // this is because we need to add a seen log entry if the enduserId is supplied
            // can't be no track
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        // TODO: Add SeenLog if optional parameter pid on behalf of end user is present
        // https://github.com/digdir/dialogporten/issues/386
        // HERRRR

        var dialogDto = _mapper.Map<GetDialogDto>(dialog);

        dialogDto.SeenSinceLastUpdate = dialog.SeenLog
            .Select(log =>
            {
                var logDto = _mapper.Map<GetDialogDialogSeenLogDto>(log);
                // TODO: Set when #386 is implemented
                // HERRR
                // logDto.IsCurrentEndUser = log.EndUserId == userPid;
                return logDto;
            })
            .ToList();

        return dialogDto;
    }
}
