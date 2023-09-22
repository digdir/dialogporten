namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<string?> GetOrgOwner(string resourceId, CancellationToken cancellationToken);
    Task<string[]> GetResourceIds(string orgNumber, CancellationToken cancellationToken);
}