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
    private readonly IUserNameRegistry _userNameRegistry;
    private readonly IStringHasher _stringHasher;

    public SearchDialogSeenLogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IAltinnAuthorization altinnAuthorization,
        IUserNameRegistry userNameRegistry,
        IStringHasher stringHasher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _userNameRegistry = userNameRegistry ?? throw new ArgumentNullException(nameof(userNameRegistry));
        _stringHasher = stringHasher ?? throw new ArgumentNullException(nameof(stringHasher));
    }

    public async Task<SearchDialogSeenLogResult> Handle(SearchDialogSeenLogQuery request, CancellationToken cancellationToken)
    {
        if (!_userNameRegistry.TryGetCurrentUserPid(out var userPid))
        {
            return new Forbidden("No valid user pid found.");
        }

        var dialog = await _db.Dialogs
            .AsNoTracking()
            .Include(x => x.SeenLog)
                .ThenInclude(x => x.Via!.Localizations)
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
                dto.IsCurrentEndUser = x.EndUserId == userPid;
                dto.EndUserIdHash = _stringHasher.Hash(x.EndUserId);
                return dto;
            })
            .ToList();
    }
}
