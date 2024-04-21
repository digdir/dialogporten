using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using MediatR;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;

public sealed class GetPartyQuery : IRequest<GetPartyDto>;

internal sealed class GetPartyQueryHandler : IRequestHandler<GetPartyQuery, GetPartyDto>
{
    private readonly IUserParties _userParties;
    private readonly IMapper _mapper;

    public GetPartyQueryHandler(IUserParties userParties, IMapper mapper)
    {
        _userParties = userParties;
        _mapper = mapper;
    }

    public async Task<GetPartyDto> Handle(GetPartyQuery request, CancellationToken cancellationToken)
    {
        var authorizedPartiesResult = await _userParties.GetUserParties(cancellationToken);
        return _mapper.Map<GetPartyDto>(authorizedPartiesResult);
    }
}
