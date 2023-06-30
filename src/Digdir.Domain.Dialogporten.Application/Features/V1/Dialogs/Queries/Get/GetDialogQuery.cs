﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;

public sealed class GetDialogQuery : IRequest<OneOf<GetDialogDto, EntityNotFound>>
{
    public Guid Id { get; set; }
}

internal sealed class GetDialogQueryHandler : IRequestHandler<GetDialogQuery, OneOf<GetDialogDto, EntityNotFound>>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;

    public GetDialogQueryHandler(IDialogDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OneOf<GetDialogDto, EntityNotFound>> Handle(GetDialogQuery request, CancellationToken cancellationToken)
    {
        // This query could be written without all the includes as ProjctTo will do the job for us.
        // However, we need to guarantee an order for sub resources of the dialog aggragate.
        // This is to ensure that the get is consistent, and that PATCH in the API presentation
        // layer behavious in an expected manner. Therefore we need to be a bit more verbose about it.
        var dialog = await _db.Dialogs
            .Include(x => x.Body.Localizations.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.Title.Localizations.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.SenderName.Localizations.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.SearchTitle.Localizations.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.Elements.OrderByDescending(x => x.CreatedAt))
                .ThenInclude(x => x.DisplayName.Localizations.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.Elements.OrderByDescending(x => x.CreatedAt))
                .ThenInclude(x => x.Urls.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.GuiActions.OrderByDescending(x => x.CreatedAt))
                .ThenInclude(x => x.Title.Localizations.OrderByDescending(x => x.CreatedAt))
            .Include(x => x.ApiActions.OrderByDescending(x => x.CreatedAt))
                .ThenInclude(x => x.Endpoints.OrderByDescending(x => x.CreatedAt))
            .AsNoTracking()
            .ProjectTo<GetDialogDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.Id);
        }

        return dialog;
    }
}
