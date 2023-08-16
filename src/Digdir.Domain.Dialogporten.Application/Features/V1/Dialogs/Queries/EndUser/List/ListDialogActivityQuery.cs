using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.ServiceOwner.Get;
using MediatR;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.List;

public sealed class ListDialogActivityQuery : DefaultPaginationParameter, IRequest<ListDialogActivityResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class ListDialogActivityResult : OneOfBase<PaginatedList<ListDialogDialogActivityDto>, ValidationError> { }

internal sealed class ListDialogActivityQueryHandler : IRequestHandler<ListDialogActivityQuery, ListDialogActivityResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;

    public ListDialogActivityQueryHandler(IDialogDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ListDialogActivityResult> Handle(ListDialogActivityQuery request, CancellationToken cancellationToken)
    {
        return await _db.DialogActivities
            .Where(x => x.DialogId.Equals(request.DialogId))
            .ProjectTo<ListDialogDialogActivityDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(x => x.CreatedAt, request, cancellationToken);
    }
}