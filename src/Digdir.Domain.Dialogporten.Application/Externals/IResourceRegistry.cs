namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<IReadOnlyCollection<ServiceResourceInformation>> GetResourceInformationForOrg(string orgNumber, CancellationToken cancellationToken);
    Task<ServiceResourceInformation?> GetResourceInformation(string serviceResourceId, CancellationToken cancellationToken);
    IAsyncEnumerable<UpdatedSubjectResource> GetUpdatedSubjectResources(DateTimeOffset since, CancellationToken cancellationToken);
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

public sealed record UpdatedSubjectResource
{
    public Uri Subject { get; set; } = null!;
    public Uri Resource { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
    public bool Deleted { get; set; }
}
