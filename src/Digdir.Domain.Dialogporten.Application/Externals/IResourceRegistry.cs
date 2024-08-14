namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<IReadOnlyCollection<string>> GetResourceIds(string orgNumber, CancellationToken cancellationToken);
    Task<string> GetResourceType(string orgNumber, string serviceResourceId, CancellationToken cancellationToken);
    Task<bool> ResourceExists(string serviceResource, CancellationToken cancellationToken);
}
