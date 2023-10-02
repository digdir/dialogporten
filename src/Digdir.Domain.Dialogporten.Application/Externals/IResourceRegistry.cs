namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<string[]> GetResourceIds(string orgNumber, CancellationToken cancellationToken);
}