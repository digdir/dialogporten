using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Get;

public sealed class GetDialogSeenLogQuery : IRequest<GetDialogSeenLogResult>
{
    public Guid DialogId { get; set; }
    public Guid SeenLogId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogSeenLogResult : OneOfBase<GetDialogSeenLogDto, EntityNotFound>;

internal sealed class GetDialogSeenLogQueryHandler : IRequestHandler<GetDialogSeenLogQuery, GetDialogSeenLogResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;
    private readonly IStringHasher _stringHasher;
    private readonly IUserResourceRegistry _userResourceRegistry;

    public GetDialogSeenLogQueryHandler(
        IMapper mapper,
        IDialogDbContext dbContext,
        IStringHasher stringHasher,
        IUserResourceRegistry userResourceRegistry)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _stringHasher = stringHasher ?? throw new ArgumentNullException(nameof(stringHasher));
        _userResourceRegistry = userResourceRegistry ?? throw new ArgumentNullException(nameof(userResourceRegistry));
    }

    public async Task<GetDialogSeenLogResult> Handle(GetDialogSeenLogQuery request,
        CancellationToken cancellationToken)
    {
        var resourceIds = await _userResourceRegistry.GetCurrentUserResourceIds(cancellationToken);

        var dialog = await _dbContext.Dialogs
            .AsNoTracking()
            .Include(x => x.SeenLog.Where(x => x.Id == request.SeenLogId))
            .IgnoreQueryFilters()
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var seenLog = dialog.SeenLog.FirstOrDefault();
        if (seenLog is null)
        {
            return new EntityNotFound<DialogSeenLog>(request.SeenLogId);
        }

        var dto = _mapper.Map<GetDialogSeenLogDto>(seenLog);
        dto.EndUserIdHash = _stringHasher.Hash(seenLog.EndUserId);

        return dto;
    }
}
