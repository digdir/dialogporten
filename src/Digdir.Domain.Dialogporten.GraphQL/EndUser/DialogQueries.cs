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
            notFound => new DialogByIdPayload { Errors = [new DialogByIdNotFound { Message = notFound.Message }] },
            deleted => new DialogByIdPayload { Errors = [new DialogByIdDeleted { Message = deleted.Message }] },
            forbidden => new DialogByIdPayload { Errors = [new DialogByIdForbidden { Message = "Forbidden" }] });
    }

    public async Task<SearchDialogsPayload> SearchDialogs(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        SearchDialogInput input,
        CancellationToken cancellationToken)
    {
        var searchDialogQuery = mapper.Map<SearchDialogQuery>(input);

        var result = await mediator.Send(searchDialogQuery, cancellationToken);

        return result.Match(
            mapper.Map<SearchDialogsPayload>,
            validationError => new SearchDialogsPayload
            {
                Errors = [.. validationError.Errors.Select(x => new SearchDialogValidationError { Message = x.ErrorMessage })]
            },
            forbidden => new SearchDialogsPayload { Errors = [new SearchDialogForbidden()] });
    }
}
