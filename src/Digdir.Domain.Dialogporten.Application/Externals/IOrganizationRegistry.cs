namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IOrganizationRegistry
{
    Task<string?> GetOrgShortName(string orgNumber, CancellationToken cancellationToken);
}
