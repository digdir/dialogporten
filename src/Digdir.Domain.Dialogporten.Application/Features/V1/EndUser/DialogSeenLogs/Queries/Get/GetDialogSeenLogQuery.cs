using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Get;

public sealed class GetDialogSeenLogQuery : IRequest<GetDialogSeenLogResult>
{
    public Guid DialogId { get; set; }
    public Guid SeenLogId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogSeenLogResult : OneOfBase<GetDialogSeenLogDto, EntityNotFound, EntityDeleted, Forbidden>;

internal sealed class GetDialogSeenLogQueryHandler : IRequestHandler<GetDialogSeenLogQuery, GetDialogSeenLogResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IStringHasher _stringHasher;
    private readonly IUserNameRegistry _userNameRegistry;

    public GetDialogSeenLogQueryHandler(
        IMapper mapper,
        IDialogDbContext dbContext,
        IAltinnAuthorization altinnAuthorization,
        IStringHasher stringHasher,
        IUserNameRegistry userNameRegistry)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _stringHasher = stringHasher ?? throw new ArgumentNullException(nameof(stringHasher));
        _userNameRegistry = userNameRegistry ?? throw new ArgumentNullException(nameof(userNameRegistry));
    }

    public async Task<GetDialogSeenLogResult> Handle(GetDialogSeenLogQuery request,
        CancellationToken cancellationToken)
    {
        if (!_userNameRegistry.TryGetCurrentUserPid(out var userPid))
        {
            return new Forbidden("No valid user pid found.");
        }

        var dialog = await _dbContext.Dialogs
            .AsNoTracking()
            .Include(x => x.SeenLog.Where(x => x.Id == request.SeenLogId))
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

        var seenLog = dialog.SeenLog.FirstOrDefault();
        if (seenLog is null)
        {
            return new EntityNotFound<DialogSeenLog>(request.SeenLogId);
        }

        var dto = _mapper.Map<GetDialogSeenLogDto>(seenLog);
        dto.IsCurrentEndUser = userPid == seenLog.EndUserId;
        dto.EndUserIdHash = _stringHasher.Hash(seenLog.EndUserId);

        return dto;
    }
}
