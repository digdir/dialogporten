namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class AuthorizedPartiesResult
{
    public List<AuthorizedParty> AuthorizedParties { get; set; } = [];
}

public class AuthorizedPartyBase
{
    public string Party { get; init; } = null!;
    public string Name { get; init; } = null!;
    public AuthorizedPartyType PartyType { get; init; }
    public bool IsDeleted { get; init; }
    public bool HasKeyRole { get; init; }
    public bool IsCurrentEndUser { get; set; }
    public bool IsMainAdministrator { get; init; }
    public bool IsAccessManager { get; init; }
    public bool HasOnlyAccessToSubParties { get; init; }
    public List<string> AuthorizedResources { get; init; } = [];
    public List<string> AuthorizedRoles { get; init; } = [];
}

public sealed class AuthorizedParty : AuthorizedPartyBase
{
    // Only populated in case of "flatten = false"
    public List<AuthorizedSubParty>? SubParties { get; set; }

    // Only populated in case of "flatten = true"
    public string? ParentParty { get; set; }
}

public sealed class AuthorizedSubParty : AuthorizedPartyBase;

public enum AuthorizedPartyType
{
    Person,
    Organization
}
