using System.Linq.Expressions;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
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
public partial class GetDialogElementResult : OneOfBase<GetDialogElementDto, EntityNotFound, EntityDeleted>;

internal sealed class GetDialogElementQueryHandler : IRequestHandler<GetDialogElementQuery, GetDialogElementResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public GetDialogElementQueryHandler(
        IMapper mapper,
        IDialogDbContext dbContext,
        IAltinnAuthorization altinnAuthorization)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
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

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken);

        // If we cannot read the dialog at all, we don't allow access to any of the dialog elements
        if (!authorizationResult.HasReadAccessToMainResource())
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
        dto.IsAuthorized = authorizationResult.HasReadAccessToDialogElement(element);

        return dto;
    }
}
