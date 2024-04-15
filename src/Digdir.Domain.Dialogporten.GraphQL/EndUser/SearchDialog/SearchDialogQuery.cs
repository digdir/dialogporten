using AutoMapper;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialog;

public interface ISearchDialogQuery
{
    Task<DialogSearch> SearchDialog(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Argument] Guid dialogId,
        CancellationToken cancellationToken);

}
