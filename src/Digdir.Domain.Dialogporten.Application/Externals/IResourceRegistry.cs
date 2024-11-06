using Digdir.Domain.Dialogporten.Domain.ResourcePolicyMetadata;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<IReadOnlyCollection<ServiceResourceInformation>> GetResourceInformationForOrg(string orgNumber, CancellationToken cancellationToken);
    Task<ServiceResourceInformation?> GetResourceInformation(string serviceResourceId, CancellationToken cancellationToken);
    IAsyncEnumerable<List<UpdatedSubjectResource>> GetUpdatedSubjectResources(DateTimeOffset since, int batchSize,
        CancellationToken cancellationToken);
    IAsyncEnumerable<List<ResourcePolicyMetadata>> GetUpdatedResourcePolicyMetadata(DateTimeOffset since, int batchSize,
        CancellationToken cancellationToken);
}

public sealed record ServiceResourceInformation
{
    public string ResourceType { get; }
    public string OwnerOrgNumber { get; }
    public string ResourceId { get; }

    public ServiceResourceInformation(string resourceId, string resourceType, string ownerOrgNumber)
    {
        ResourceId = resourceId.ToLowerInvariant();
        ResourceType = resourceType.ToLowerInvariant();
        OwnerOrgNumber = ownerOrgNumber.ToLowerInvariant();
    }
}


public sealed record UpdatedSubjectResource(Uri SubjectUrn, Uri ResourceUrn, DateTimeOffset UpdatedAt, bool Deleted);
