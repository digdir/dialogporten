using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.List;

public sealed class ListDialogQuery : DefaultPaginationParameter, IRequest<ListDialogResult> { }

[GenerateOneOf]
public partial class ListDialogResult : OneOfBase<PaginatedList<ListDialogDto>, ValidationError> { }

internal sealed class ListDialogQueryHandler : IRequestHandler<ListDialogQuery, ListDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public ListDialogQueryHandler(IDialogDbContext db, IMapper mapper, IClock clock)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<ListDialogResult> Handle(ListDialogQuery request, CancellationToken cancellationToken)
    {
        return await _db.Dialogs
            .Where(x => !x.VisibleFrom.HasValue || _clock.UtcNowOffset < x.VisibleFrom)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<ListDialogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken);
    }
}
