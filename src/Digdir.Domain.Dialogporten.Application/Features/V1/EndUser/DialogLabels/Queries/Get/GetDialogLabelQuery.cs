using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabels.Queries.Get;

public sealed class GetDialogLabelQuery : IRequest<GetDialogLabelResult>
{
    public Guid DialogId { get; set; }
}

// Amund: List eller PaginatedList? ser begge deler blir brukt gir egt ikke mening med paging her? blir vell aldri så mange labels?
[GenerateOneOf]
public sealed partial class GetDialogLabelResult : OneOfBase<List<GetDialogLabelDto>, EntityNotFound, EntityDeleted, Forbidden>;

internal sealed class GetDialogLabelQueryHandler : IRequestHandler<GetDialogLabelQuery, GetDialogLabelResult>
{
    private readonly IMapper _mapper;
    private readonly IDialogDbContext _dbContext;
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IUserRegistry _userRegistry;

    public GetDialogLabelQueryHandler(IMapper mapper, IDialogDbContext dbContext, IAltinnAuthorization altinnAuthorization, IUserRegistry userRegistry)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
    }
    public async Task<GetDialogLabelResult> Handle(GetDialogLabelQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserInformation = await _userRegistry.GetCurrentUserInformation(cancellationToken);
        var dialog = await _dbContext.Dialogs
            .AsNoTracking()
            // .Include(x => x.Labels)
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);
        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken: cancellationToken);

        // If we cannot access the dialog at al, we don't allow access to the labels
        if (!authorizationResult.HasAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }
        // return dialog.Labels.Select(x =>
        // {
        //     var dto = _mapper.Map<GetDialogLabelDto>(x);
        //     return dto;
        // }).ToList();
        return null!;
    }
}
