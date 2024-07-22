using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using OneOf;
using Microsoft.EntityFrameworkCore;
using Digdir.Domain.Dialogporten.Application.Common;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Search;

public sealed class SearchDialogSeenLogQuery : IRequest<SearchDialogSeenLogResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class SearchDialogSeenLogResult : OneOfBase<List<SearchDialogSeenLogDto>, EntityNotFound, EntityDeleted, Forbidden>;

internal sealed class SearchDialogSeenLogQueryHandler : IRequestHandler<SearchDialogSeenLogQuery, SearchDialogSeenLogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IUserRegistry _userRegistry;

    public SearchDialogSeenLogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IAltinnAuthorization altinnAuthorization,
        IUserRegistry userRegistry)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
    }

    public async Task<SearchDialogSeenLogResult> Handle(SearchDialogSeenLogQuery request, CancellationToken cancellationToken)
    {
        var currentUserInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);

        var dialog = await _db.Dialogs
            .AsNoTracking()
            .Include(x => x.SeenLog)
            .ThenInclude(x => x.SeenBy)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken);

        // If we cannot read the dialog at all, we don't allow access to the seen log
        if (!authorizationResult.HasReadAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        return dialog.SeenLog
            .Select(x =>
            {
                var dto = _mapper.Map<SearchDialogSeenLogDto>(x);
                dto.IsCurrentEndUser = currentUserInformation.UserId.ExternalIdWithPrefix == x.SeenBy.ActorId;
                return dto;
            })
            .ToList();
    }
}
