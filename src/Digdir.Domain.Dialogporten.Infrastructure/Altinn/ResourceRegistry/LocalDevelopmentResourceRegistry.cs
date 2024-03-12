using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class LocalDevelopmentResourceRegistry : IResourceRegistry
{
    private static readonly HashSet<string> _cachedResourceIds = [];
    private readonly IDialogDbContext _db;

    public LocalDevelopmentResourceRegistry(IDialogDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyCollection<string>> GetResourceIds(string orgNumber, CancellationToken cancellationToken)
    {
        var newIds = await _db.Dialogs
            .Where(x => !_cachedResourceIds.Contains(x.ServiceResource))
            .Select(x => x.ServiceResource)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var id in newIds)
        {
            _cachedResourceIds.Add(id);
        }

        return _cachedResourceIds;
    }
}
