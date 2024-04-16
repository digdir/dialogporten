namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IPartyNameRegistry
{
    Task<string?> GetPersonName(string personalIdentificationNumber, CancellationToken cancellationToken);
    Task<string?> GetOrganizationName(string organizationNumber, CancellationToken cancellationToken);
}
