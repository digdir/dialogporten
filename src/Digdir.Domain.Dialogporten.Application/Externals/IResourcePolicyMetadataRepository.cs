using Digdir.Domain.Dialogporten.Domain.ResourcePolicyMetadata;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourcePolicyMetadataRepository
{
    Task<int> Merge(IReadOnlyCollection<ResourcePolicyMetadata> resourceMetadata, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> GetLastUpdatedAt(TimeSpan? timeSkew = null, CancellationToken cancellationToken = default);
}

public static class ResourcePolicyMetadataExtensions
{
    public static ResourcePolicyMetadata ToResourcePolicyMetadata(this UpdatedResourcePolicyMetadata updatedResourcePolicyMetadata, DateTimeOffset createdAt)
    {
        return new ResourcePolicyMetadata
        {
            Id = IdentifiableExtensions.CreateVersion7(),
            Resource = updatedResourcePolicyMetadata.ResourceUrn.ToString()!,
            MinimumSecurityLevel = updatedResourcePolicyMetadata.MinimumSecurityLevel,
            CreatedAt = createdAt.ToUniversalTime(),
            UpdatedAt = updatedResourcePolicyMetadata.UpdatedAt.ToUniversalTime()
        };
    }
}
