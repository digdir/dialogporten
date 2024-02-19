using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.Search;

public sealed class SearchDialogActivityQuery : IRequest<SearchDialogActivityResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class SearchDialogActivityResult : OneOfBase<List<SearchDialogActivityDto>, EntityNotFound>;

internal sealed class SearchDialogActivityQueryHandler : IRequestHandler<SearchDialogActivityQuery, SearchDialogActivityResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public SearchDialogActivityQueryHandler(IDialogDbContext db, IMapper mapper, IUserService userService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userService = userService;
    }

    public async Task<SearchDialogActivityResult> Handle(SearchDialogActivityQuery request, CancellationToken cancellationToken)
    {
        var resourceIds = await _userService.GetCurrentUserResourceIds(cancellationToken);

        var dialog = await _db.Dialogs
            .Include(x => x.Activities)
            .IgnoreQueryFilters()
            .Where(x => resourceIds.Contains(x.ServiceResource))
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        return _mapper.Map<List<SearchDialogActivityDto>>(dialog.Activities);
    }
}
