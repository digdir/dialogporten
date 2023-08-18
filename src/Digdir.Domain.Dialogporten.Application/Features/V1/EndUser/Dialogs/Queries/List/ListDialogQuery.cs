using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.List;

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
            .ProjectTo<ListDialogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(x => x.CreatedAt, request, cancellationToken);
    }
}
