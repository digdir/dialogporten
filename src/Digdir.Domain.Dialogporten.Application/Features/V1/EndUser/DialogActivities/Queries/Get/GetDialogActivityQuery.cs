using AutoMapper;
using AutoMapper.QueryableExtensions;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
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
public partial class GetDialogActivityResult : OneOfBase<GetDialogActivityDto, EntityNotFound>
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
        var dto = await _dbContext.DialogActivities
            .ProjectTo<GetDialogActivityDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.DialogId == request.DialogId && x.Id == request.ActivityId,
                cancellationToken: cancellationToken);

        if (dto is null)
        {
            return new EntityNotFound<DialogActivity>(request.ActivityId);
        }

        return dto;
    }
}