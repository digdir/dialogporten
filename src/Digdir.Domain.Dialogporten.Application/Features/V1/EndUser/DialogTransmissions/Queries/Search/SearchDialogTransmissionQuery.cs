using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Search;

public sealed class SearchDialogTransmissionQuery : IRequest<SearchDialogTransmissionResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public partial class SearchDialogTransmissionResult : OneOfBase<List<SearchDialogTransmissionDto>, EntityNotFound, EntityDeleted>;

internal sealed class SearchDialogTransmissionQueryHandler : IRequestHandler<SearchDialogTransmissionQuery, SearchDialogTransmissionResult>
{
    private readonly IDialogDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public SearchDialogTransmissionQueryHandler(IDialogDbContext db, IMapper mapper, IAltinnAuthorization altinnAuthorization)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
    }

    public async Task<SearchDialogTransmissionResult> Handle(SearchDialogTransmissionQuery request, CancellationToken cancellationToken)
    {
        var dialog = await _db.Dialogs
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Content.OrderBy(x => x.Id).ThenBy(x => x.CreatedAt))
                .ThenInclude(x => x.Value.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Attachments.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.DisplayName!.Localizations.OrderBy(x => x.CreatedAt).ThenBy(x => x.LanguageCode))
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Attachments.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
                .ThenInclude(x => x.Urls.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            .Include(x => x.Transmissions)
                .ThenInclude(x => x.Sender)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.DialogId,
                cancellationToken: cancellationToken);

        if (dialog is null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(
            dialog,
            cancellationToken: cancellationToken);

        // If we cannot read the dialog at all, we don't allow access to any of the activity history
        if (!authorizationResult.HasReadAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }

        return _mapper.Map<List<SearchDialogTransmissionDto>>(dialog.Transmissions);
    }
}
