namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IOrganizationRegistry
{
    Task<OrganizationInfo?> GetOrgInfo(string orgNumber, CancellationToken cancellationToken);
}

public sealed class OrganizationInfo
{
    public required string OrgNumber { get; init; }
    public required string ShortName { get; init; }
}