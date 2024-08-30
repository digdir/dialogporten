using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Medo;
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

    public async Task RunAsync(DateTimeOffset? since = null, CancellationToken cancellationToken = default)
    {
        await SyncResourceSubjects(since, cancellationToken);
    }

    private async Task SyncResourceSubjects(DateTimeOffset? since = null, CancellationToken cancellationToken = default)
    {
        // 1. Get the last updated timestamp from parameter, or the database, or use a default
        var lastUpdated = since ?? _db.SubjectResourceLastUpdates.OrderByDescending(x => x.LastUpdate)
            .FirstOrDefault()?.LastUpdate ?? DateTimeOffset.MinValue;

        // Allow for a few seconds of drift between server clocks
        var newLastUpdated = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(3));

        _logger.LogInformation("Fetching updated subject resources since {LastUpdated}", lastUpdated);

        // 2. Get the resources from the resource registry, supplying the last updated as "since"
        var remoteUpdatedSubjectResources =
            await _resourceRegistry.GetUpdatedSubjectResources(lastUpdated, cancellationToken);

        _logger.LogInformation("Fetched {Count} updated subject resources", remoteUpdatedSubjectResources.Count);

        // 3. Delete the subjectResources that are no longer in the resource registry
        var remoteDeletedSubjectResources = remoteUpdatedSubjectResources
            .Where(x => x.Deleted)
            .Select(deletedSubjectResource => new
            {
                deletedSubjectResource.Resource,
                deletedSubjectResource.Subject
            })
            .ToList();

        if (remoteDeletedSubjectResources.Count != 0)
        {
            var resourceStrings = remoteDeletedSubjectResources.Select(r => r.Resource.ToString()).ToList();
            var subjectStrings = remoteDeletedSubjectResources.Select(r => r.Subject.ToString()).ToList();

            var localSubjectResourcesToDelete = _db.SubjectResources
                .Where(x => resourceStrings.Contains(x.Resource) && subjectStrings.Contains(x.Subject))
                .ToList();

            if (localSubjectResourcesToDelete.Count != 0)
            {
                _logger.LogInformation("Deleting {Count} subject resources", localSubjectResourcesToDelete.Count);
                _db.SubjectResources.RemoveRange(localSubjectResourcesToDelete);
            }
        }

        // 4. Add the ones that are new (or no longer deleted)
        var remoteNewSubjectResources = remoteUpdatedSubjectResources
            .Where(x => !x.Deleted)
            .Select(updatedSubjectResource => new SubjectResource
            {
                Id = Uuid7.NewUuid7().ToGuid(matchGuidEndianness: true),
                Resource = updatedSubjectResource.Resource.ToString(),
                Subject = updatedSubjectResource.Subject.ToString(),
                // We use the updated timestamp from the remote API as our CreatedAt timestamp
                // UpdatedAt at our end is the time we received the data
                CreatedAt = updatedSubjectResource.UpdatedAt
            })
            .ToList();

        if (remoteNewSubjectResources.Count != 0)
        {
            // FIXME! This is horribly inefficient as the table grows
            var localExistingResources = _db.SubjectResources
                .Select(x => new { x.Resource, x.Subject })
                .ToHashSet();

            var localResourcesToAdd = remoteNewSubjectResources
                .Where(newResource => !localExistingResources.Contains(new { newResource.Resource, newResource.Subject }))
                .ToList();

            if (localResourcesToAdd.Count != 0)
            {
                _logger.LogInformation("Adding {Count} new subject resources", localResourcesToAdd.Count);
                await _db.SubjectResources.AddRangeAsync(localResourcesToAdd, cancellationToken);
            }
        }

        // 5. Update the last updated timestamp in the database if we have any changes
        if (remoteUpdatedSubjectResources.Count != 0)
        {
            _logger.LogInformation("Updating the last updated timestamp to {LastUpdated}", newLastUpdated);
            var lastUpdateRecord = _db.SubjectResourceLastUpdates.FirstOrDefault();
            if (lastUpdateRecord is null)
            {
                await _db.SubjectResourceLastUpdates.AddAsync(new SubjectResourceLastUpdate
                {
                    LastUpdate = newLastUpdated
                }, cancellationToken);
            }
            else
            {
                lastUpdateRecord.LastUpdate = newLastUpdated;
                _db.SubjectResourceLastUpdates.Update(lastUpdateRecord);
            }
        }

        // 6. Save changes to the database
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Subject resources synced successfully");
    }
}
