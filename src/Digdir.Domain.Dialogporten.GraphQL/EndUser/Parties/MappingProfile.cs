using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Parties;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuthorizedPartyDto, AuthorizedParty>();
    }
}
