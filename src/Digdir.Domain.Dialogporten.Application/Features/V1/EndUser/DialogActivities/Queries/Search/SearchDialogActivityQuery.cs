using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.Search;

public sealed class SearchDialogActivityQuery : IRequest<SearchDialogActivityResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class SearchDialogActivityResult : OneOfBase<List<SearchDialogActivityDto>, EntityNotFound, EntityDeleted>;

internal sealed class SearchDialogActivityQueryHandler : IRequestHandler<SearchDialogActivityQuery, SearchDialogActivityResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public SearchDialogActivityQueryHandler(
        IDialogDbContext db,
        IMapper mapper,
        IAltinnAuthorization altinnAuthorization)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<SearchDialogActivityResult> Handle(SearchDialogActivityQuery request, CancellationToken cancellationToken)
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

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken);

        // If we cannot read the dialog at all, we don't allow access to any of the activity history
        if (!authorizationResult.HasReadAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        return _mapper.Map<List<SearchDialogActivityDto>>(dialog.Activities);
    }
}
