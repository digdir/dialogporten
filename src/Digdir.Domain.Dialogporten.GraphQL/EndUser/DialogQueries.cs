using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser;

public partial class Queries
{
    public async Task<DialogByIdPayload> GetDialogById(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Argument] Guid dialogId,
        CancellationToken cancellationToken)
    {
        var request = new GetDialogQuery { DialogId = dialogId };
        var result = await mediator.Send(request, cancellationToken);
        return result.Match(
            dialog => new DialogByIdPayload { Dialog = mapper.Map<Dialog>(dialog) },
            // TODO: Error handling
            notFound => new DialogByIdPayload { Errors = ["Not found"] },
            deleted => new DialogByIdPayload { Errors = ["Deleted"] },
            forbidden => new DialogByIdPayload { Errors = ["Forbidden"] });
    }

    public async Task<SearchDialogsPayload> SearchDialogs(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        SearchDialogInput input,
        CancellationToken cancellationToken)
    {

        var searchDialogQuery = mapper.Map<SearchDialogQuery>(input);

        var result = await mediator.Send(searchDialogQuery, cancellationToken);

        var searchResultOneOf = result.Match(
            paginatedList => paginatedList,
            // TODO: Error handling
            validationError => throw new NotImplementedException("Validation error"),
            forbidden => throw new NotImplementedException("Forbidden"));

        var dialogSearchResult = mapper.Map<SearchDialogsPayload>(searchResultOneOf);

        return dialogSearchResult;
    }
}
