using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class LocalDevelopmentResourceRegistry : IResourceRegistry
{
    private const string LocalResourceType = "LocalResourceType";
    private static readonly HashSet<ServiceResourceInformation> CachedResourceIds = new(new ServiceResourceInformationEqualityComparer());
    private readonly IDialogDbContext _db;

    public LocalDevelopmentResourceRegistry(IDialogDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyCollection<ServiceResourceInformation>> GetResourceInformationForOrg(string orgNumber, CancellationToken cancellationToken)
    {
        var newIds = await _db.Dialogs
            .Select(x => x.ServiceResource)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var id in newIds)
        {
            CachedResourceIds.Add(new ServiceResourceInformation(id, LocalResourceType, orgNumber));
        }

        return CachedResourceIds;
    }

    // TODO: Local testing of correspondence?
    public Task<string> GetResourceType(string _, string __, CancellationToken ___)
        => Task.FromResult(LocalResourceType);

    public Task<ServiceResourceInformation?> GetResourceInformation(string serviceResourceId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private sealed class ServiceResourceInformationEqualityComparer : IEqualityComparer<ServiceResourceInformation>
    {
        public bool Equals(ServiceResourceInformation? x, ServiceResourceInformation? y)
            => x?.ResourceId == y?.ResourceId && x?.OwnerOrgNumber == y?.OwnerOrgNumber;

        public int GetHashCode(ServiceResourceInformation obj)
            => HashCode.Combine(obj.ResourceId, obj.OwnerOrgNumber);
    }
}
