using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.List;

public sealed class ListDialogueQuery : DefaultPaginationParameter, IRequest<OneOf<PaginatedList<ListDialogueDto>, ValidationError>> { }

internal sealed class ListDialogueQueryHandler : IRequestHandler<ListDialogueQuery, OneOf<PaginatedList<ListDialogueDto>, ValidationError>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;

    public ListDialogueQueryHandler(IDialogueDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OneOf<PaginatedList<ListDialogueDto>, ValidationError>> Handle(ListDialogueQuery request, CancellationToken cancellationToken)
    {
        return await _db.Dialogues
            .AsNoTracking()
            .OrderByDescending(x => x.InternalId)
            .ProjectTo<ListDialogueDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken);
    }
}
