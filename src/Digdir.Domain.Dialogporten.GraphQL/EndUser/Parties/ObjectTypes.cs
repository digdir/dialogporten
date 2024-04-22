namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Parties;

public class AuthorizedParty
{
    public string Party { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public bool HasKeyRole { get; init; }
    public bool IsMainAdministrator { get; init; }
    public bool IsAccessManager { get; init; }
    public bool HasOnlyAccessToSubParties { get; init; }
    public List<AuthorizedParty>? SubParties { get; init; }
}
