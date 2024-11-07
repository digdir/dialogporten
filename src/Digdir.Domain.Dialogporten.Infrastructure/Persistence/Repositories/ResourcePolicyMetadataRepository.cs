using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.ResourcePolicyMetadata;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Repositories;

internal sealed class ResourcePolicyMetadataRepository : IResourcePolicyMetadataRepository
{
    private readonly DialogDbContext _dbContext;

    public ResourcePolicyMetadataRepository(DialogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<DateTimeOffset> GetLastUpdatedAt(
        TimeSpan? timeSkew = null,
        CancellationToken cancellationToken = default)
    {
        var lastUpdatedAt = await _dbContext.ResourcePolicyMetadata
            .Select(x => x.UpdatedAt)
            .DefaultIfEmpty()
            .MaxAsync(cancellationToken);

        return timeSkew.HasValue
            ? lastUpdatedAt.Add(timeSkew.Value)
            : lastUpdatedAt;
    }

    public Task<DateTimeOffset> GetLastUpdatedAt(CancellationToken cancellationToken) =>
        _dbContext.ResourcePolicyMetadata
            .Select(x => x.UpdatedAt)
            .DefaultIfEmpty()
            .MaxAsync(cancellationToken);

    public async Task<int> Merge(IReadOnlyCollection<ResourcePolicyMetadata> resourceMetadata, CancellationToken cancellationToken)
    {
        const string sql =
            $"""
            with source as (
            	SELECT *
            	FROM unnest(@ids, @resources, @minimumSecurityLevels, @createdAts, @updatedAts) 
            	    as s(id, resource, minimumSecurityLevel, createdAt, updatedAt)
            )
            merge into "{nameof(ResourcePolicyMetadata)}" t
            using source s
            on t."{nameof(ResourcePolicyMetadata.Resource)}" = s.resource
            when matched then
              	update set "{nameof(ResourcePolicyMetadata.UpdatedAt)}" = s.updatedAt
            when not matched then
              	insert ("{nameof(ResourcePolicyMetadata.Id)}", "{nameof(ResourcePolicyMetadata.Resource)}", "{nameof(ResourcePolicyMetadata.MinimumSecurityLevel)}", "{nameof(ResourcePolicyMetadata.CreatedAt)}", "{nameof(ResourcePolicyMetadata.UpdatedAt)}")
              	values (s.id, s.resource, s.minimumSecurityLevel, s.createdAt, s.updatedAt);
            """;

        return resourceMetadata.Count == 0 ? 0
            : await _dbContext.Database.ExecuteSqlRawAsync(sql,
            [
                new NpgsqlParameter("ids", resourceMetadata.Select(x => x.Id).ToArray()),
                new NpgsqlParameter("resources", resourceMetadata.Select(x => x.Resource).ToArray()),
                new NpgsqlParameter("minimumSecurityLevels", resourceMetadata.Select(x => x.MinimumSecurityLevel).ToArray()),
                new NpgsqlParameter("createdAts", resourceMetadata.Select(x => x.CreatedAt).ToArray()),
                new NpgsqlParameter("updatedAts", resourceMetadata.Select(x => x.UpdatedAt).ToArray())
            ], cancellationToken);
    }
}
