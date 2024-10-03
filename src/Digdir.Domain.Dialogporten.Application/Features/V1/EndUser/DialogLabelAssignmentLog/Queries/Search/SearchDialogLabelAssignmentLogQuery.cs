using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssignmentLog.Queries.Search;

public sealed class SearchDialogLabelAssignmentLogQuery : IRequest<SearchDialogLabelAssignmentLogResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public sealed partial class SearchDialogLabelAssignmentLogResult : OneOfBase<List<SearchDialogLabelAssignmentLogDto>, EntityNotFound, EntityDeleted>;

internal sealed class SearchDialogLabelAssignmentLogQueryHandler : IRequestHandler<SearchDialogLabelAssignmentLogQuery, SearchDialogLabelAssignmentLogResult>
{
    private readonly IDialogDbContext _dialogDbContext;
    private readonly IMapper _mapper;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public SearchDialogLabelAssignmentLogQueryHandler(IDialogDbContext dialogDbContext, IMapper mapper, IAltinnAuthorization altinnAuthorization)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<SearchDialogLabelAssignmentLogResult> Handle(SearchDialogLabelAssignmentLogQuery request, CancellationToken cancellationToken)
    {
        var dialog = await _dialogDbContext.Dialogs
            .AsNoTracking()
            .Include(x => x.DialogEndUserContext)
                .ThenInclude(x => x.LabelAssignmentLogs)
                .ThenInclude(x => x.PerformedBy)
            .FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken: cancellationToken);

        if (dialog == null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(dialog, cancellationToken: cancellationToken);
        if (!authorizationResult.HasAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        return _mapper.Map<List<SearchDialogLabelAssignmentLogDto>>(dialog.DialogEndUserContext.LabelAssignmentLogs);
    }
}
