namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IPersonNameRegistry
{
    Task<string?> GetName(string personalIdentificationNumber, CancellationToken cancellationToken);
}
