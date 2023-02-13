using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.List;

// TODO: Add pagination?
public class ListDialogueQuery : IRequest<List<ListDialogueDto>>
{
    public string Title { get; set; }
}

internal sealed class ListDialogueQueryHandler : IRequestHandler<ListDialogueQuery, List<ListDialogueDto>>
{
    private readonly IDialogueDbContext _db;
    private readonly IMapper _mapper;

    public ListDialogueQueryHandler(IDialogueDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<ListDialogueDto>> Handle(ListDialogueQuery request, CancellationToken cancellationToken)
    {
        //var query = _db.Dialogues.AsNoTracking();

        //if (!string.IsNullOrWhiteSpace(request.Title))
        //{
        //    query = query.Where(x => x.Title.Contains(request.Title.Trim()));
        //}

        //return await query
        //    .OrderBy(x => x.Title)
        //    .ProjectTo<ListDialogueDto>(_mapper.ConfigurationProvider)
        //    .ToListAsync(cancellationToken);
        throw new NotImplementedException();
    }
}
