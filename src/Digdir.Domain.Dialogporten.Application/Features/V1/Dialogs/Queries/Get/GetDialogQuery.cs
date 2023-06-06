using AutoMapper;
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
        var dialog = await _db.Dialogs
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
