namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IOrganizationRegistry
{
    Task<string?> GetOrgShortName(string orgNumber, CancellationToken cancellationToken);
    Task<OrganizationLongName[]> GetOrganizationLongNames(string orgNumber, CancellationToken cancellationToken);
}

public sealed class OrganizationLongName
{
    public required string LongName { get; init; }
    public required Language Language { get; init; }
}

public enum Language
{
    No, // Norwegian Bokmål
    En, // English
    Nn  // Norwegian Nynorsk
}