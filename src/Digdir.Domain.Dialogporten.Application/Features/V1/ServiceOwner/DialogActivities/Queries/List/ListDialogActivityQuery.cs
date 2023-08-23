using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.List;

public sealed class ListDialogActivityQuery : IRequest<ListDialogActivityResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class ListDialogActivityResult : OneOfBase<List<ListDialogActivityDto>, EntityNotFound, EntityDeleted> { }

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
        var dialog = await _db.Dialogs
            .Include(x => x.Activities)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, 
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        return _mapper.Map<List<ListDialogActivityDto>>(dialog.Activities);
    }
}