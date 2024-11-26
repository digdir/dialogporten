namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;

public sealed class PartiesDto
{
    public List<AuthorizedPartyDto> AuthorizedParties { get; init; } = [];
}

public class AuthorizedPartyBaseDto
{
    public string Party { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public bool HasKeyRole { get; init; }
    public bool IsCurrentEndUser { get; init; }
    public bool IsMainAdministrator { get; init; }
    public bool IsAccessManager { get; init; }
    public bool HasOnlyAccessToSubParties { get; init; }
}

public sealed class AuthorizedPartyDto : AuthorizedPartyBaseDto
{
    public List<AuthorizedSubPartyDto>? SubParties { get; init; }
}

public sealed class AuthorizedSubPartyDto : AuthorizedPartyBaseDto;
