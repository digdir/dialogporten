namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<IReadOnlyCollection<string>> GetResourceIds(string orgNumber, CancellationToken cancellationToken);
}