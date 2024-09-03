using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;

public class SynchronizeSubjectResourceMappingsCommand : IRequest<SynchronizeResourceRegistryResult>
{
    public DateTimeOffset? Since { get; set; }
    public int? BatchSize { get; set; }
}

[GenerateOneOf]
public partial class SynchronizeResourceRegistryResult : OneOfBase<Success, ValidationError>;

internal sealed class SynchronizeResourceRegistryCommandHandler : IRequestHandler<SynchronizeSubjectResourceMappingsCommand, SynchronizeResourceRegistryResult>
{
    private const int DefaultBatchSize = 1000;
    private readonly IDialogDbContext _dialogDbContext;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly ISubjectResourceRepository _subjectResourceRepository;
    private readonly ILogger<SynchronizeResourceRegistryCommandHandler> _logger;

    public SynchronizeResourceRegistryCommandHandler(
        IDialogDbContext dialogDbContext,
        IResourceRegistry resourceRegistry,
        ISubjectResourceRepository subjectResourceRepository,
        ILogger<SynchronizeResourceRegistryCommandHandler> logger)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
        _subjectResourceRepository = subjectResourceRepository ?? throw new ArgumentNullException(nameof(subjectResourceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SynchronizeResourceRegistryResult> Handle(SynchronizeSubjectResourceMappingsCommand request, CancellationToken cancellationToken)
    {
        // 1. Get the last updated timestamp from parameter, or the database, or use a default
        var lastUpdated = request.Since
            ?? await _dialogDbContext.SubjectResources
                .Select(x => x.UpdatedAt)
                .DefaultIfEmpty()
                .MaxAsync(cancellationToken);

        _logger.LogInformation("Fetching updated subject resources since {LastUpdated:O}.", lastUpdated);

        var mergeCount = 0;
        try
        {
            await foreach (var resourceBatch in _resourceRegistry.GetUpdatedSubjectResources(lastUpdated, request.BatchSize ?? DefaultBatchSize, cancellationToken))
            {
                var created = DateTimeOffset.Now;
                var mergeableSubjectResources = resourceBatch
                    .Select(x => x.ToMergableSubjectResource(created))
                    .ToList();
                mergeCount += await _subjectResourceRepository.Merge(mergeableSubjectResources, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to sync subject-resources. {UpdatedAmount} subject-resources were synced before the error occurred.", mergeCount);
            throw;
        }

        if (mergeCount > 0)
        {
            _logger.LogInformation("Successfully synced {UpdatedAmount} subject resources.", mergeCount);
        }

        return new Success();
    }
}
