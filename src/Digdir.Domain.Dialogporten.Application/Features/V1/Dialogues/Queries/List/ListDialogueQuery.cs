using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.List;

public class ListDialogueQuery : DefaultPaginationParameter, IRequest<PaginatedList<ListDialogueDto>> { }

internal sealed class ListDialogueQueryHandler : IRequestHandler<ListDialogueQuery, PaginatedList<ListDialogueDto>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;

    public ListDialogueQueryHandler(IDialogueDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaginatedList<ListDialogueDto>> Handle(ListDialogueQuery request, CancellationToken cancellationToken)
    {
        return await _db.Dialogues
            .AsNoTracking()
            .OrderByDescending(x => x.InternalId)
            .ProjectTo<ListDialogueDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken);
    }
}
