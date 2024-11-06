using Digdir.Domain.Dialogporten.Domain.ResourcePolicyMetadata;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourcePolicyMetadataRepository
{
    Task<int> Merge(List<ResourcePolicyMetadata> resourceMetadata, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> GetLastUpdatedAt(TimeSpan? timeSkew = null, CancellationToken cancellationToken = default);
}
