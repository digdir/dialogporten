using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.Parties;
using MediatR;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser;

public partial class Queries
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public async Task<List<AuthorizedParty>> GetParties(
        [Service] ISender mediator,
        [Service] IMapper mapper,
        [Service] ILogger<Queries> logger,
        [Service] IUser user,
        CancellationToken cancellationToken)
    {
        var request = new GetPartiesQuery();
        var result = await mediator.Send(request, cancellationToken);

        user.GetPrincipal().TryGetPid(out var pid);
        logger.LogInformation("GraphQL handler, app result for party {Party}: {AuthorizedParties}",
            pid, JsonSerializer.Serialize(result, SerializerOptions));

        return mapper.Map<List<AuthorizedParty>>(result.AuthorizedParties);
    }
}
