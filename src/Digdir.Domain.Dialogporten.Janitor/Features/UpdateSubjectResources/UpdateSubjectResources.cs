using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Janitor.Features.UpdateSubjectResources;

internal sealed class UpdateSubjectResources
{
    private readonly IDialogDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly ILogger<UpdateSubjectResources> _logger;

    public UpdateSubjectResources(
        ILogger<UpdateSubjectResources> logger,
        IResourceRegistry resourceRegistry,
        IDialogDbContext db,
        IUnitOfWork unitOfWork)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await SyncResourceSubjects(cancellationToken);
    }

    private async Task SyncResourceSubjects(CancellationToken cancellationToken)
    {
        // 1. Get the last updated timestamp from the database
        var lastUpdated = _db.SubjectResourceLastUpdates
            .FirstOrDefault()?.LastUpdate ?? DateTime.MinValue;

        // 2. Get the resources from the resource registry, supplying the last updated as since
        var updatedSubjectResources =
            await _resourceRegistry.GetUpdatedSubjectResources(lastUpdated, cancellationToken);

        // 3. Delete the subjectResources that are no longer in the resource registry
        var resourcesToDelete = updatedSubjectResources
            .Where(x => x.Deleted)
            .Select(deletedSubjectResource => new
            {
                deletedSubjectResource.Resource,
                deletedSubjectResource.Subject
            })
            .ToList();

        if (resourcesToDelete.Count != 0)
        {
            var subjectResourcesToDelete = _db.SubjectResources
                .Where(x => resourcesToDelete
                    .Any(deletedResource =>
                        x.Resource == deletedResource.Resource.ToString() &&
                        x.Subject == deletedResource.Subject.ToString()));

            _db.SubjectResources.RemoveRange(subjectResourcesToDelete);
        }

        // 4. Add the ones that are new (handle duplicates)
        var newSubjectResources = updatedSubjectResources
            .Where(x => !x.Deleted)
            .Select(updatedSubjectResource => new SubjectResource
            {
                Resource = updatedSubjectResource.Resource.ToString(),
                Subject = updatedSubjectResource.Subject.ToString()
            })
            .ToList();

        var existingResources = _db.SubjectResources
            .Select(x => new { x.Resource, x.Subject })
            .ToHashSet();

        var resourcesToAdd = newSubjectResources
            .Where(newResource => !existingResources.Contains(new { newResource.Resource, newResource.Subject }))
            .ToList();

        if (resourcesToAdd.Count != 0)
        {
            await _db.SubjectResources.AddRangeAsync(resourcesToAdd, cancellationToken);
        }

        // 5. Update the last updated timestamp in the database
        var latestUpdatedSubjectResource = updatedSubjectResources.MaxBy(resource => resource.UpdatedAt);
        if (latestUpdatedSubjectResource is not null)
        {
            var lastUpdateRecord = _db.SubjectResourceLastUpdates.FirstOrDefault();
            if (lastUpdateRecord is null)
            {
                await _db.SubjectResourceLastUpdates.AddAsync(new SubjectResourceLastUpdate
                {
                    LastUpdate = latestUpdatedSubjectResource.UpdatedAt
                }, cancellationToken);
            }
            else
            {
                lastUpdateRecord.LastUpdate = latestUpdatedSubjectResource.UpdatedAt;
                _db.SubjectResourceLastUpdates.Update(lastUpdateRecord);
            }
        }

        // 6. Save changes to the database
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
