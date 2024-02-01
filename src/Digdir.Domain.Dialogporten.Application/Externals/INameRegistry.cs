namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface INameRegistry
{
    Task<string?> GetName(string personalIdentificationNumber, CancellationToken cancellationToken);
}
