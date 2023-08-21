using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.Get;

public sealed class GetDialogActivityQuery : IRequest<GetDialogActivityResult>
{
    public Guid DialogId { get; set; }
    public Guid ActivityId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogActivityResult : OneOfBase<GetDialogActivityDto, EntityNotFound, EntityDeleted>
{
}

internal sealed class GetDialogActivityQueryHandler : IRequestHandler<GetDialogActivityQuery, GetDialogActivityResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;

    public GetDialogActivityQueryHandler(IMapper mapper, IDialogDbContext dbContext)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<GetDialogActivityResult> Handle(GetDialogActivityQuery request,
        CancellationToken cancellationToken)
    {
        var dialog = await _dbContext.Dialogs
            .Include(x => x.Activities.Where(x => x.Id == request.ActivityId))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, 
                cancellationToken: cancellationToken);

        if (dialog is null)
            return new EntityNotFound<DialogEntity>(request.DialogId);

        if (dialog.Deleted)
            return new EntityDeleted<DialogEntity>(request.DialogId);
        
        var activity = dialog.Activities.FirstOrDefault();

        if(activity is null)
            return new EntityNotFound<DialogActivity>(request.ActivityId);
        
        return _mapper.Map<GetDialogActivityDto>(activity);
    }
}