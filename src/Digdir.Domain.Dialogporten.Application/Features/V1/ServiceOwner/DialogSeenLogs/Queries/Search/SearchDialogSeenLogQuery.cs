﻿using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;

public sealed class SearchDialogSeenLogQuery : IRequest<SearchDialogSeenLogResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public sealed partial class SearchDialogSeenLogResult : OneOfBase<List<SearchSeenLogDto>, EntityNotFound>;

internal sealed class SearchDialogSeenLogQueryHandler : IRequestHandler<SearchDialogSeenLogQuery, SearchDialogSeenLogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUserResourceRegistry _userResourceRegistry;

    public SearchDialogSeenLogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IUserResourceRegistry userResourceRegistry)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userResourceRegistry = userResourceRegistry;
    }

    public async Task<SearchDialogSeenLogResult> Handle(SearchDialogSeenLogQuery request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userResourceRegistry.GetCurrentUserResourceIds(cancellationToken);

        var dialog = await _db.Dialogs
            .AsNoTracking()
            .Include(x => x.SeenLog)
                .ThenInclude(x => x.SeenBy)
            .IgnoreQueryFilters()
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        return dialog.SeenLog
            .Select(x =>
            {
                var dto = _mapper.Map<SearchSeenLogDto>(x);
                return dto;
            })
            .ToList();
    }
}
