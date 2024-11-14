using Digdir.Domain.Dialogporten.Domain.ResourcePolicy;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourcePolicyRepository
{
    Task<int> Merge(IReadOnlyCollection<ResourcePolicy> resourceMetadata, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> GetLastUpdatedAt(TimeSpan? timeSkew = null, CancellationToken cancellationToken = default);
}

public static class ResourcePolicyExtensions
{
    public static ResourcePolicy ToResourcePolicy(this UpdatedResourcePolicy updatedResourcePolicy, DateTimeOffset createdAt)
    {
        return new ResourcePolicy
        {
            Id = IdentifiableExtensions.CreateVersion7(),
            Resource = updatedResourcePolicy.ResourceUrn.ToString()!,
            MinimumAuthenticationLevel = updatedResourcePolicy.MinimumAuthenticationLevel,
            CreatedAt = createdAt.ToUniversalTime(),
            UpdatedAt = updatedResourcePolicy.UpdatedAt.ToUniversalTime()
        };
    }
}
