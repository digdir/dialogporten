using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using OneOf;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.ServiceOwner.List;

public sealed class ListDialogQuery : DefaultPaginationParameter, IRequest<ListDialogResult> { }

public sealed class ListDialogQueryOrder : IDefaultOrder<ListDialogQueryOrder, ListDialogDto>
{
    public Expression<Func<ListDialogDto, object?>> OrderBy { get; init; } = null!;
    public OrderDirection Direction { get; init; }

    public static Expression<Func<ListDialogDto, object?>> GetIdExpression() => x => x.Id;
    public static Expression<Func<ListDialogDto, object?>> GetDefaultOrderExpression() => x => x.CreatedAt;
}

[GenerateOneOf]
public partial class ListDialogResult : OneOfBase<PaginatedList<ListDialogDto>, ValidationError> { }

internal sealed class ListDialogQueryHandler : IRequestHandler<ListDialogQuery, ListDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;

    public ListDialogQueryHandler(IDialogDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ListDialogResult> Handle(ListDialogQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //return await _db.Dialogs
        //    .ProjectTo<ListDialogDto>(_mapper.ConfigurationProvider)
        //    .ToPaginatedListAsync(IDefaultOrder<ListDialogQueryOrder, ListDialogDto>.Default, request, cancellationToken);
    }
}
