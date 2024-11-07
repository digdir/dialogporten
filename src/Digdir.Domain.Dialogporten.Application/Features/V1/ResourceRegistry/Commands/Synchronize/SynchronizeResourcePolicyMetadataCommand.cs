using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;

public sealed class SynchronizeResourcePolicyMetadataCommand : IRequest<SynchronizeResourcePolicyMetadataResult>
{
    public DateTimeOffset? Since { get; set; }
    public int? NumberOfConcurrentRequests { get; set; }
}

[GenerateOneOf]
public sealed partial class SynchronizeResourcePolicyMetadataResult : OneOfBase<Success, ValidationError>;

internal class SynchronizeResourcePolicyMetadataCommandHandler : IRequestHandler<SynchronizeResourcePolicyMetadataCommand, SynchronizeResourcePolicyMetadataResult>
{
    private const int DefaultNumberOfConcurrentRequests = 15;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly IResourcePolicyMetadataRepository _resourcePolicyMetadataRepository;
    private readonly ILogger<SynchronizeResourcePolicyMetadataCommandHandler> _logger;

    public SynchronizeResourcePolicyMetadataCommandHandler(
        IResourceRegistry resourceRegistry,
        IResourcePolicyMetadataRepository resourcePolicyMetadataRepository,
        ILogger<SynchronizeResourcePolicyMetadataCommandHandler> logger)
    {
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
        _resourcePolicyMetadataRepository = resourcePolicyMetadataRepository ?? throw new ArgumentNullException(nameof(resourcePolicyMetadataRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SynchronizeResourcePolicyMetadataResult> Handle(SynchronizeResourcePolicyMetadataCommand request,
        CancellationToken cancellationToken)
    {
        // Get the last updated timestamp from parameter, or the database (with a time skew), or use a default
        var lastUpdated = request.Since
                          ?? await _resourcePolicyMetadataRepository.GetLastUpdatedAt(
                              timeSkew: TimeSpan.FromMicroseconds(1),
                              cancellationToken: cancellationToken);

        _logger.LogInformation("Fetching updated resource policy metadata since {LastUpdated:O}.", lastUpdated);

        try
        {
            var syncTime = DateTimeOffset.Now;
            var updatedResourcePolicyMetadata = await _resourceRegistry
                               .GetUpdatedResourcePolicyMetadata(lastUpdated, request.NumberOfConcurrentRequests ?? DefaultNumberOfConcurrentRequests, cancellationToken);

            var mergeableResourcePolicyMetadata = updatedResourcePolicyMetadata
                .Select(x => x.ToResourcePolicyMetadata(syncTime))
                .ToList();
            var mergeCount = await _resourcePolicyMetadataRepository.Merge(mergeableResourcePolicyMetadata, cancellationToken);
            _logger.LogInformation("{MergeCount} sets of resource policy metadata updated.", mergeCount);

            if (mergeCount > 0)
            {
                _logger.LogInformation("Successfully synced {UpdatedAmount} total resource policy metadata.", mergeCount);
            }
            else
            {
                _logger.LogInformation("Resource policy metadata are already up-to-date.");
            }

            return new Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to sync resource policy metadata.");
            throw;
        }
    }
}
