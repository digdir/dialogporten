namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public class AuthorizedPartiesResult
{
    public List<AuthorizedParty> AuthorizedParties { get; init; } = new();
}

public class AuthorizedParty
{
    public string Party { get; init; } = null!;
    public string Name { get; init; } = null!;
    public AuthorizedPartyType PartyType { get; init; }
    public bool IsDeleted { get; init; }
    public bool HasKeyRole { get; init; }
    public bool IsMainAdministrator { get; init; }
    public bool IsAccessManager { get; init; }
    public bool HasOnlyAccessToSubParties { get; init; }
    public List<string> AuthorizedResources { get; init; } = new();
    public List<AuthorizedParty>? SubParties { get; init; }
}

public enum AuthorizedPartyType
{
    Person,
    Organization
}
