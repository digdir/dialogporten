using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Search;

public sealed class SearchDialogElementQuery : IRequest<SearchDialogElementResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class SearchDialogElementResult : OneOfBase<List<SearchDialogElementDto>, EntityNotFound> { }

internal sealed class SearchDialogElementQueryHandler : IRequestHandler<SearchDialogElementQuery, SearchDialogElementResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;

    public SearchDialogElementQueryHandler(IDialogDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SearchDialogElementResult> Handle(SearchDialogElementQuery request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .Include(x => x.Elements)
            .ThenInclude(x => x.DisplayName!.Localizations)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        return _mapper.Map<List<SearchDialogElementDto>>(dialog.Elements);
    }
}
