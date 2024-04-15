using AutoMapper;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;

public interface ISearchDialogQuery
{
    Task<SearchDialogsPayload> SearchDialogs(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        SearchDialogInput input,
        CancellationToken cancellationToken);
}
