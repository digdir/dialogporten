using System.Linq.Expressions;
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
    public Guid ElementId { get; set; }
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
        Expression<Func<DialogEntity, IEnumerable<DialogElement>>> elementFilter = dialog =>
            dialog.Elements.Where(x => x.Id == request.ElementId);
        
        var dialog = await _dbContext.Dialogs
            .Include(elementFilter)
                .ThenInclude(x => x.DisplayName!.Localizations)
            .Include(elementFilter)
                .ThenInclude(x => x.Urls)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, 
                cancellationToken: cancellationToken);
        
        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }
        
        var element = dialog.Elements.FirstOrDefault();

        if (element is null)
        {
            return new EntityNotFound<DialogElement>(request.ElementId);
        }
        
        var dto = _mapper.Map<GetDialogElementDto>(element);
        return dto;
    }
}