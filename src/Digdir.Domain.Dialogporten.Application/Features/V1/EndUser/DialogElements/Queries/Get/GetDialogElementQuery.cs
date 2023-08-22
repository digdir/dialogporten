using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Get;

public sealed class GetDialogElementQuery : IRequest<GetDialogElementResult>
{
    public Guid DialogId { get; set; }
    public Guid DialogElementId { get; set; }
}

[GenerateOneOf]
public partial class GetDialogElementResult : OneOfBase<GetDialogElementDto, EntityNotFound, EntityDeleted>
{
}

internal sealed class GetDialogElementQueryHandler : IRequestHandler<GetDialogElementQuery, GetDialogElementResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;

    public GetDialogElementQueryHandler(IMapper mapper, IDialogDbContext dbContext)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<GetDialogElementResult> Handle(GetDialogElementQuery request,
        CancellationToken cancellationToken)
    {
        var dialog = await _dbContext.Dialogs
            .Include(x => x.Elements.Where(x => x.Id == request.DialogElementId))
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Elements.Where(x => x.Id == request.DialogElementId))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.Elements.Where(x => x.Id == request.DialogElementId))
                .ThenInclude(x => x.RelatedDialogElements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.Activities.Where(x => x.DialogElementId == request.DialogElementId))
                .ThenInclude(x => x.Description!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.Activities.Where(x => x.DialogElementId == request.DialogElementId))
                .ThenInclude(x => x.PerformedBy!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.CultureCode))
            .Include(x => x.ApiActions.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Endpoints.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, 
                cancellationToken: cancellationToken);

        if (dialog is null)
            return new EntityNotFound<DialogEntity>(request.DialogId);

        if (dialog.Deleted)
            return new EntityDeleted<DialogEntity>(request.DialogId);
        
        var element = dialog.Elements.FirstOrDefault();

        if(element is null)
            return new EntityNotFound<DialogElement>(request.DialogElementId);
        
        var dto = _mapper.Map<GetDialogElementDto>(element);
        return dto;
    }
}