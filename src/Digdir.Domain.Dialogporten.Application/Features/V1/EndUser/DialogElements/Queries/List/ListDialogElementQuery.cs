using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.List;

public sealed class ListDialogElementQuery : IRequest<ListDialogElementResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class ListDialogElementResult : OneOfBase<List<ListDialogElementDto>, EntityNotFound, EntityDeleted> { }

internal sealed class ListDialogElementQueryHandler : IRequestHandler<ListDialogElementQuery, ListDialogElementResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;

    public ListDialogElementQueryHandler(IDialogDbContext db, IMapper mapper)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ListDialogElementResult> Handle(ListDialogElementQuery request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .Include(x => x.Elements)
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, 
                cancellationToken: cancellationToken);

        if (dialog is null)
            return new EntityNotFound<DialogEntity>(request.DialogId);

        if (dialog.Deleted)
            return new EntityDeleted<DialogEntity>(request.DialogId);
        
        return _mapper.Map<List<ListDialogElementDto>>(dialog.Elements);
    }
}