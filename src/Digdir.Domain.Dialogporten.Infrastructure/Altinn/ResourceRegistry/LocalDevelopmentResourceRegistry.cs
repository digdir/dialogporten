using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class LocalDevelopmentResourceRegistry : IResourceRegistry
{
    private const string LocalResourceType = "LocalResourceType";
    private const string LocalOrgId = "742859274";
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

    public Task<ServiceResourceInformation?> GetResourceInformation(string serviceResourceId, CancellationToken cancellationToken)
    {
        return Task.FromResult<ServiceResourceInformation?>(
            new ServiceResourceInformation(serviceResourceId, LocalResourceType, LocalOrgId));
    }

#pragma warning disable CA1822
    public async IAsyncEnumerable<UpdatedSubjectResource> GetUpdatedSubjectResources(DateTimeOffset _, [EnumeratorCancellation] CancellationToken __)
#pragma warning restore CA1822
    {
        await Task.CompletedTask;
        yield break;
    }

    private sealed class ServiceResourceInformationEqualityComparer : IEqualityComparer<ServiceResourceInformation>
    {
        public bool Equals(ServiceResourceInformation? x, ServiceResourceInformation? y)
            => x?.ResourceId == y?.ResourceId && x?.OwnerOrgNumber == y?.OwnerOrgNumber;

        public int GetHashCode(ServiceResourceInformation obj)
            => HashCode.Combine(obj.ResourceId, obj.OwnerOrgNumber);
    }
}
