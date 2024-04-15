using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialog;
using HotChocolate.Authorization;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser;

[Authorize(Policy = AuthorizationPolicy.EndUser)]
public class DialogQueries : ISearchDialogQuery, IDialogByIdQuery
{
    public async Task<Dialog> GetDialogById(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Argument] Guid dialogId,
        CancellationToken cancellationToken)
    {
        var request = new GetDialogQuery { DialogId = dialogId };
        var result = await mediator.Send(request, cancellationToken);
        var foo = result.Match(
            dialog => dialog,
            notFound => throw new NotImplementedException("Not found"),
            deleted => throw new NotImplementedException("Deleted"),
            forbidden => throw new NotImplementedException("Forbidden"));

        var dialog = mapper.Map<Dialog>(foo);

        return dialog;
        // return foo;
    }

    public async Task<DialogSearch> SearchDialog(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Argument] Guid dialogId,
        CancellationToken cancellationToken)
    {
        var request = new GetDialogQuery { DialogId = dialogId };
        var result = await mediator.Send(request, cancellationToken);
        var foo = result.Match(
            dialog => dialog,
            notFound => throw new NotImplementedException("Not found"),
            deleted => throw new NotImplementedException("Deleted"),
            forbidden => throw new NotImplementedException("Forbidden"));

        var dialog = mapper.Map<DialogSearch>(foo);

        return dialog;
        // return foo;
    }
}
