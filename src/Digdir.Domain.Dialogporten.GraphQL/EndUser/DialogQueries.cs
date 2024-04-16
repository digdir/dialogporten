using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;
using HotChocolate.Authorization;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser;

[Authorize(Policy = AuthorizationPolicy.EndUser)]
public class DialogQueries
{
    public async Task<Dialog> GetDialogById(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Argument] Guid dialogId,
        CancellationToken cancellationToken)
    {
        var request = new GetDialogQuery { DialogId = dialogId };
        var result = await mediator.Send(request, cancellationToken);
        var getDialogResult = result.Match(
            dialog => dialog,
            // TODO: Error handling
            notFound => throw new NotImplementedException("Not found"),
            deleted => throw new NotImplementedException("Deleted"),
            forbidden => throw new NotImplementedException("Forbidden"));

        var dialog = mapper.Map<Dialog>(getDialogResult);

        return dialog;
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
