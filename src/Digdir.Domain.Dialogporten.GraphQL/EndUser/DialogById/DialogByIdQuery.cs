using AutoMapper;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

public interface IDialogByIdQuery
{
    Task<Dialog> GetDialogById(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Argument] Guid dialogId,
        CancellationToken cancellationToken);
}
