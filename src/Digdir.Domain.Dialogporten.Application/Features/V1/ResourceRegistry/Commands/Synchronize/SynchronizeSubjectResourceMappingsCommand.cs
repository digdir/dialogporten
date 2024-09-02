using System.Data;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using OneOf;
using OneOf.Types;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;

public class SynchronizeSubjectResourceMappingsCommand : IRequest<SynchronizeResourceRegistryResult>
{
    public DateTimeOffset? Since { get; set; }
}

[GenerateOneOf]
public partial class SynchronizeResourceRegistryResult : OneOfBase<Success, ValidationError>;

internal sealed class SynchronizeResourceRegistryCommandHandler : IRequestHandler<SynchronizeSubjectResourceMappingsCommand, SynchronizeResourceRegistryResult>
{
    private const int DatabaseMaxBatchSize = 200;
    private const string Schema = "public";
    private const string SubjectResourceLastUpdateTable = nameof(SubjectResourceLastUpdate);
    private const string LastUpdateColumn = nameof(SubjectResourceLastUpdate.LastUpdate);
    private const string IdColumn = nameof(SubjectResourceLastUpdate.Id);
    private const string SubjectResourceTable = nameof(SubjectResource);
    private const string ResourceColumn = nameof(SubjectResource.Resource);
    private const string SubjectColumn = nameof(SubjectResource.Subject);
    private const string CreatedAtColumn = nameof(SubjectResource.CreatedAt);
    private const string UpdatedAtColumn = nameof(SubjectResource.UpdatedAt);

    private readonly IDialogDbContext _dialogDbContext;
    private readonly IResourceRegistry _resourceRegistry;
    private readonly ILogger<SynchronizeResourceRegistryCommandHandler> _logger;

    public SynchronizeResourceRegistryCommandHandler(
        IDialogDbContext dialogDbContext,
        IResourceRegistry resourceRegistry,
        ILogger<SynchronizeResourceRegistryCommandHandler> logger)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SynchronizeResourceRegistryResult> Handle(SynchronizeSubjectResourceMappingsCommand request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_dialogDbContext.Database.GetConnectionString());
        await conn.OpenAsync(cancellationToken);

        // 1. Get the last updated timestamp from parameter, or the database, or use a default
        var lastUpdated = request.Since ?? await GetLastUpdated(conn, cancellationToken);
        _logger.LogInformation("Last updated: {LastUpdated}", lastUpdated.ToString($"O"));

        // Allow for a few seconds of drift between server clocks
        var newLastUpdated = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(3));

        _logger.LogInformation("Fetching updated subject resources since {LastUpdated}", lastUpdated);

        // 2. Start a transaction to ensure that we can perform all changes atomically
        await using var tx = await conn.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);

        var remoteUpdatedSubjectResources = new List<UpdatedSubjectResource>();
        var numDeleted = 0;
        var numUpserted = 0;
        await foreach (var resource in _resourceRegistry.GetUpdatedSubjectResources(lastUpdated, cancellationToken))
        {
            // Batch results to avoid too many database round trips
            remoteUpdatedSubjectResources.Add(resource);
            if (remoteUpdatedSubjectResources.Count < DatabaseMaxBatchSize) continue;

            // 3. Delete the subjectResources that are no longer in the resource registry
            numDeleted += await HandleDeletedSubjectResources(conn, tx, remoteUpdatedSubjectResources, cancellationToken);

            // 4. Add the ones that are new (or no longer deleted)
            numUpserted += await HandleNewOrUpdatedSubjectResources(conn, tx, remoteUpdatedSubjectResources, cancellationToken);
            remoteUpdatedSubjectResources.Clear();
        }

        // Process any remaining resources that didn't fit into a full batch
        if (remoteUpdatedSubjectResources.Count > 0)
        {
            numDeleted += await HandleDeletedSubjectResources(conn, tx, remoteUpdatedSubjectResources, cancellationToken);
            numUpserted += await HandleNewOrUpdatedSubjectResources(conn, tx, remoteUpdatedSubjectResources, cancellationToken);
        }

        // 5. Update the last updated timestamp in the database if we have any changes
        if (numDeleted > 0 || numUpserted > 0)
        {
            _logger.LogInformation("Deleted {NumDeleted} and inserted/updated {NumUpserted} subject resources", numDeleted, numUpserted);
            _logger.LogInformation("Setting last updated timestamp to {NewLastUpdated}", newLastUpdated.ToString($"O"));
            await SetLastUpdated(conn, tx, newLastUpdated, cancellationToken);
        }

        // 6. Commit changes to the database
        await tx.CommitAsync(cancellationToken);
        _logger.LogInformation("Subject resources synced successfully");

        return new Success();
    }

    private static async Task<DateTimeOffset> GetLastUpdated(NpgsqlConnection conn, CancellationToken cancellationToken)
    {
        const string sql = $"""
                            SELECT "{LastUpdateColumn}" FROM {Schema}."{SubjectResourceLastUpdateTable}" LIMIT 1
                            """;
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.PrepareAsync(cancellationToken);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? await reader.GetFieldValueAsync<DateTimeOffset>(0, cancellationToken: cancellationToken)
            : DateTimeOffset.MinValue;
    }

    private static async Task SetLastUpdated(NpgsqlConnection conn, NpgsqlTransaction tx, DateTimeOffset newLastUpdated, CancellationToken cancellationToken)
    {
        var knownUuid = new Guid("deadbeef-dead-beef-dead-beefdeadbeef"); // Fixed guid to ensure only one row is updated
        const string sql = $"""
                            INSERT INTO {Schema}."{SubjectResourceLastUpdateTable}" ("{IdColumn}", "{LastUpdateColumn}")
                            VALUES (@id, @lastUpdate)
                            ON CONFLICT ("{IdColumn}") 
                            DO UPDATE SET "{LastUpdateColumn}" = EXCLUDED."{LastUpdateColumn}";
                            """;

        await using var cmd = new NpgsqlCommand(sql, conn, tx);

        cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = knownUuid });
        cmd.Parameters.Add(new NpgsqlParameter("lastUpdate", NpgsqlDbType.TimestampTz) { Value = newLastUpdated.ToUniversalTime() });

        await cmd.PrepareAsync(cancellationToken);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> HandleDeletedSubjectResources(NpgsqlConnection conn, NpgsqlTransaction tx, List<UpdatedSubjectResource> remoteUpdatedSubjectResources, CancellationToken cancellationToken)
    {
        const string sql = $"""
                            DELETE FROM {Schema}."{SubjectResourceTable}" WHERE ("{ResourceColumn}", "{SubjectColumn}") IN (
                                SELECT UNNEST(@resources), UNNEST(@subjects)
                            )
                            """;

        var resources = remoteUpdatedSubjectResources
            .Where(x => x.Deleted)
            .Select(deletedSubjectResource => deletedSubjectResource.Resource.ToString())
            .ToArray();

        if (resources.Length == 0)
        {
            return 0;
        }

        var subjects = remoteUpdatedSubjectResources
            .Where(x => x.Deleted)
            .Select(deletedSubjectResource => deletedSubjectResource.Subject.ToString())
            .ToArray();

        await using var cmd = new NpgsqlCommand(sql, conn, tx);

        cmd.Parameters.Add(new NpgsqlParameter("@resources", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = resources });
        cmd.Parameters.Add(new NpgsqlParameter("@subjects", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = subjects });

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> HandleNewOrUpdatedSubjectResources(NpgsqlConnection conn, NpgsqlTransaction tx, List<UpdatedSubjectResource> remoteUpdatedSubjectResources, CancellationToken cancellationToken)
    {
        const string sql =
            $"""
             WITH new_data AS (
                 SELECT 
                     unnest(@ids) AS {IdColumn},
                     unnest(@resources) AS {ResourceColumn}, 
                     unnest(@subjects) AS {SubjectColumn}, 
                     unnest(@createdAtTimes) AS {CreatedAtColumn}
             )
             INSERT INTO {Schema}."{SubjectResourceTable}" ("{IdColumn}", "{ResourceColumn}", "{SubjectColumn}", "{CreatedAtColumn}", "{UpdatedAtColumn}")
             SELECT {IdColumn}, {ResourceColumn}, {SubjectColumn}, {CreatedAtColumn}, NOW()
             FROM new_data
             ON CONFLICT ("{ResourceColumn}", "{SubjectColumn}") 
             DO UPDATE SET "{CreatedAtColumn}" = EXCLUDED."{CreatedAtColumn}", "{UpdatedAtColumn}" = NOW()
             """;

        var newOrUpdatedSubjectResources = remoteUpdatedSubjectResources
            .Where(x => !x.Deleted)
            .ToList();

        if (remoteUpdatedSubjectResources.Count == 0)
        {
            return 0;
        }

        var ids = newOrUpdatedSubjectResources
            .Select(_ => new Guid?().CreateVersion7IfDefault())
            .ToArray();

        var resources = newOrUpdatedSubjectResources
            .Select(x => x.Resource.ToString())
            .ToArray();

        var subjects = newOrUpdatedSubjectResources
            .Select(x => x.Subject.ToString())
            .ToArray();

        var createdAtTimes = newOrUpdatedSubjectResources
            .Select(x => x.UpdatedAt.ToUniversalTime())
            .ToArray();

        await using var cmd = new NpgsqlCommand(sql, conn, tx);

        cmd.Parameters.Add(new NpgsqlParameter("@ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = ids });
        cmd.Parameters.Add(new NpgsqlParameter("@resources", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = resources });
        cmd.Parameters.Add(new NpgsqlParameter("@subjects", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = subjects });
        cmd.Parameters.Add(new NpgsqlParameter("@createdAtTimes", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = createdAtTimes });

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
