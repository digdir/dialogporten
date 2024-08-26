using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.SubjectResourceSync.Features.UpdateSubjectResources;

internal sealed class UpdateSubjectResources
{
    private readonly IDialogDbContext _dialogDbContext;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly ILogger _logger;

    public UpdateSubjectResources(
        ILoggerFactory loggerFactory,
        IResourceRegistry resourceRegistry,
        IDialogDbContext dialogDbContext)
    {
        _dialogDbContext = dialogDbContext;
        _resourceRegistry = resourceRegistry;
        _logger = loggerFactory.CreateLogger<UpdateSubjectResources>();
    }

    [Function("UpdateSubjectResources")]
    public async Task RunAsync([TimerTrigger("0 */1 * * * *")] CancellationToken cancellationToken)
    {
        await SyncResourceSubjects(cancellationToken);
    }

    private async Task SyncResourceSubjects(CancellationToken cancellationToken)
    {
        // 1. Get the last updated timestamp from the database
        var lastUpdated = _dialogDbContext.SubjectResourceLastUpdates
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
            var subjectResourcesToDelete = _dialogDbContext.SubjectResources
                .Where(x => resourcesToDelete
                    .Any(deletedResource =>
                        x.Resource == deletedResource.Resource.ToString() &&
                        x.Subject == deletedResource.Subject.ToString()));

            _dialogDbContext.SubjectResources.RemoveRange(subjectResourcesToDelete);
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

        var existingResources = _dialogDbContext.SubjectResources
            .Select(x => new { x.Resource, x.Subject })
            .ToHashSet();

        var resourcesToAdd = newSubjectResources
            .Where(newResource => !existingResources.Contains(new { newResource.Resource, newResource.Subject }))
            .ToList();

        if (resourcesToAdd.Count != 0)
        {
            await _dialogDbContext.SubjectResources.AddRangeAsync(resourcesToAdd, cancellationToken);
        }

        // 5. Update the last updated timestamp in the database
        var latestUpdatedSubjectResource = updatedSubjectResources.MaxBy(resource => resource.UpdatedAt);
        if (latestUpdatedSubjectResource is not null)
        {
            var lastUpdateRecord = _dialogDbContext.SubjectResourceLastUpdates.FirstOrDefault();
            if (lastUpdateRecord is null)
            {
                _dialogDbContext.SubjectResourceLastUpdates.Add(new SubjectResourceLastUpdate
                {
                    LastUpdate = latestUpdatedSubjectResource.UpdatedAt
                });
            }
            else
            {
                lastUpdateRecord.LastUpdate = latestUpdatedSubjectResource.UpdatedAt;
                _dialogDbContext.SubjectResourceLastUpdates.Update(lastUpdateRecord);
            }
        }

        // 6. Save changes to the database
        //await _dialogDbContext.SaveChangesAsync(cancellationToken);
    }
}
