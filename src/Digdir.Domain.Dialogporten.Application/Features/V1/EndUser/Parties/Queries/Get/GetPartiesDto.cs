namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;

public class GetPartiesDto
{
    public List<AuthorizedPartyDto> AuthorizedParties { get; init; } = new();
}

public class AuthorizedPartyDto
{
    public string Party { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public bool HasKeyRole { get; init; }
    public bool IsMainAdministrator { get; init; }
    public bool IsAccessManager { get; init; }
    public bool HasOnlyAccessToSubParties { get; init; }
    public List<AuthorizedPartyDto>? SubParties { get; init; }
}
