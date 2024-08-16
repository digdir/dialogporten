namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IResourceRegistry
{
    Task<IReadOnlyCollection<ServiceResourceInformation>> GetResourceInformationForOrg(string orgNumber, CancellationToken cancellationToken);
    Task<ServiceResourceInformation?> GetResourceInformation(string serviceResourceId, CancellationToken cancellationToken);
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
