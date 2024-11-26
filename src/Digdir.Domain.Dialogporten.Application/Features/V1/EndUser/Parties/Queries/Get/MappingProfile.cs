using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuthorizedPartiesResult, PartiesDto>();
        CreateMap<AuthorizedParty, AuthorizedPartyDto>();
        CreateMap<AuthorizedSubParty, AuthorizedSubPartyDto>();
    }
}
