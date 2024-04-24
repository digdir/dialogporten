using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.Parties;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser;

public partial class Queries
{
    public async Task<List<AuthorizedParty>> GetParties(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        CancellationToken cancellationToken)
    {
        var request = new GetPartiesQuery();
        var result = await mediator.Send(request, cancellationToken);

        return mapper.Map<List<AuthorizedParty>>(result.AuthorizedParties);
    }
}
