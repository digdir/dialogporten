using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.Pagination.OrderOption;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using MediatR;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.List;

public sealed class ListDialogQuery : PaginationParameter<ListDialogQueryOrderDefinition, ListDialogDto>, IRequest<ListDialogResult> { }

public sealed class ListDialogQueryOrderDefinition : IOrderDefinition<ListDialogDto>
{
    static IOrderOptions<ListDialogDto> IOrderDefinition<ListDialogDto>.Configure(IOrderOptionsBuilder<ListDialogDto> options) => options
        .AddId(x => x.Id)
        .AddDefault("createdAt", x => x.CreatedAt)
        .Build();
}

[GenerateOneOf]
public partial class ListDialogResult : OneOfBase<PaginatedList<ListDialogDto>, ValidationError> { }

internal sealed class ListDialogQueryHandler : IRequestHandler<ListDialogQuery, ListDialogResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly IUser _user;

    public ListDialogQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IResourceRegistry resourceRegistry,
        IUser user)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }

    public async Task<ListDialogResult> Handle(ListDialogQuery request, CancellationToken cancellationToken)
    {
        var currentUser = _user.GetPrincipal();
        var orgNumber = currentUser.Claims
            .First(x => x.Type == "consumer")
            .Properties
            .TryGetValue("ID", out var iso6523OrgNumber);

        var resourceIds = await _resourceRegistry.GetResourceIds();

        return await _db.Dialogs
            .ProjectTo<ListDialogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request, cancellationToken);
    }
}
