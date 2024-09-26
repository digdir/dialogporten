using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssigmentLog.Queries.Search;

public sealed class SearchDialogLabelAssignmentLogQuery : IRequest<SearchDialogLabelAssignmentLogResult>
{
    public Guid DialogId { get; set; }
}

[GenerateOneOf]
public sealed partial class SearchDialogLabelAssignmentLogResult : OneOfBase<List<SearchDialogLabelAssignmentLogDto>, EntityNotFound, EntityDeleted, Forbidden>;

internal sealed class SearchDialogLabelAssignmentLogQueryHandler(IDialogDbContext dialogDbContext, IMapper mapper, IAltinnAuthorization altinnAuthorization) : IRequestHandler<SearchDialogLabelAssignmentLogQuery, SearchDialogLabelAssignmentLogResult>
{
    private readonly IDialogDbContext _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IAltinnAuthorization _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));

    public async Task<SearchDialogLabelAssignmentLogResult> Handle(SearchDialogLabelAssignmentLogQuery request, CancellationToken cancellationToken)
    {
        var dialog = await _dialogDbContext.Dialogs.Include(x => x.DialogEndUserContext).FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken: cancellationToken);
        if (dialog == null)
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        var authorizationResult = await _altinnAuthorization.GetDialogDetailsAuthorization(dialog, cancellationToken: cancellationToken);
        if (authorizationResult.HasAccessToMainResource())
        {
            return new EntityNotFound<DialogEntity>(request.DialogId);
        }

        if (dialog.Deleted)
        {
            return new EntityDeleted<DialogEntity>(request.DialogId);
        }
        return dialog.DialogEndUserContext.LabelAssignmentLogs.Select(_mapper.Map<SearchDialogLabelAssignmentLogDto>).ToList();
        // var dialogEndUserContext = await _dialogDbContext.DialogEndUserContexts.Include(x => x.LabelAssignmentLogs)
        //     .FirstOrDefaultAsync(x => x.DialogId == request.DialogId,åå
        //         cancellationToken: cancellationToken);
        // if (dialogEndUserContext is null)
        // {
        //     // Magnus: vil dette gi dårlig feilmelding? jeg sier at jeg ikke finner dialogen men det er dialogEndUserContext jeg faktisk ikke fant.
        //     // det skal jo være ganske det samme egt. men det er da ikke faktisk det samme
        //     return new EntityNotFound<DialogEndUserContext>(request.DialogId);
        // }
        // // Amund: auth uten selve dialogen? kan det gjøres? 
        // // samme med deleted kanskje jeg må ha hele dialogen? hmmm liker ikke 
        // return dialogEndUserContext.LabelAssignmentLogs.ToList();
    }
}
