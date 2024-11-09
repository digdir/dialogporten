using Digdir.Domain.Dialogporten.Domain.ResourcePolicyInformation;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourcePolicyInformationRepository
{
    Task<int> Merge(IReadOnlyCollection<ResourcePolicyInformation> resourceMetadata, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> GetLastUpdatedAt(TimeSpan? timeSkew = null, CancellationToken cancellationToken = default);
}

public static class ResourcePolicyInformationExtensions
{
    public static ResourcePolicyInformation ToResourcePolicyInformation(this UpdatedResourcePolicyInformation updatedResourcePolicyInformation, DateTimeOffset createdAt)
    {
        return new ResourcePolicyInformation
        {
            Id = IdentifiableExtensions.CreateVersion7(),
            Resource = updatedResourcePolicyInformation.ResourceUrn.ToString()!,
            MinimumSecurityLevel = updatedResourcePolicyInformation.MinimumSecurityLevel,
            CreatedAt = createdAt.ToUniversalTime(),
            UpdatedAt = updatedResourcePolicyInformation.UpdatedAt.ToUniversalTime()
        };
    }
}
