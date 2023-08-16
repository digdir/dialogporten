using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.Get;

public sealed class GetDialogActivityQuery : IRequest<GetDialogActivityResult>
{
    public Guid DialogId { get; set; }
    public Guid ActivityId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogActivityResult : OneOfBase<GetDialogDialogActivityDto, EntityNotFound>
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
        var dialogActivity = await _dbContext.DialogActivities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DialogId.Equals(request.DialogId) && x.Id.Equals(request.ActivityId),
                cancellationToken: cancellationToken);

        if (dialogActivity is null)
        {
            return new EntityNotFound<DialogActivity>(request.ActivityId);
        }

        var dto = _mapper.Map<GetDialogDialogActivityDto>(dialogActivity);

        return dto;
    }
}