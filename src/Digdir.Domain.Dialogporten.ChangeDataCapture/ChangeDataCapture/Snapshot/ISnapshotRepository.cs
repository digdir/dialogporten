using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal interface ISnapshotRepository
{
    Task EnsureSnapshotCheckpointTableExists(CancellationToken ct = default);
    Task<List<SnapshotCheckpoint>> GetCheckpoints(CancellationToken ct = default);
    Task<bool> TryUpsertCheckpoints(ICollection<SnapshotCheckpoint> checkpoints, CancellationToken ct = default);
    IAsyncEnumerable<NpgsqlDataReader> ReadExistingRowsFromSnapshot(string snapshotName, string tableName, SnapshotCheckpoint checkpoint, int batchSize, CancellationToken ct = default);
}

internal sealed class SnapshotRepository : ISnapshotRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<SnapshotRepository> _logger;

    public SnapshotRepository(
        NpgsqlDataSource dataSource,
        ILogger<SnapshotRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureSnapshotCheckpointTableExists(CancellationToken ct = default)
    {
        // TODO: Dette burde ligge sammen med OutboxMessage opprettelsen i infrastruktur via EF
        await using var command = _dataSource.CreateCommand(
            $"""
            CREATE TABLE IF NOT EXISTS cdc_snapshot_checkpoint (
              slot_name VARCHAR(255) PRIMARY KEY,
              confirmed_at timestamp with time zone,
              confirmed_id UUID
            );
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<List<SnapshotCheckpoint>> GetCheckpoints(CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand("SELECT slot_name, confirmed_at, confirmed_id FROM cdc_snapshot_checkpoint");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<SnapshotCheckpoint>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(new(
                await reader.GetFieldValueAsync<string>("slot_name", ct),
                await reader.GetFieldValueAsync<DateTimeOffset>("confirmed_at", ct),
                await reader.GetFieldValueAsync<Guid>("confirmed_id", ct)));
        }

        return result;
    }

    public async Task<bool> TryUpsertCheckpoints(ICollection<SnapshotCheckpoint> checkpoints, CancellationToken ct = default)
    {
        if (checkpoints.Count == 0)
        {
            return true;
        }

        const int numberOfRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(2);
        using var periodicTimer = new PeriodicTimer(retryDelay);

        for (var attempt = 0; attempt < numberOfRetries; attempt++)
        {
            await periodicTimer.WaitForNextTickAsync(ct);

            try
            {
                await UpsertCheckpoints_Private(checkpoints, ct);
                return true;
            }
            catch (Exception ex)
            {
                if (attempt + 1 < numberOfRetries)
                {
                    _logger.LogWarning(ex, "Failed to set snapshot checkpoint. Retrying in {RetryDelay}.", retryDelay);
                }
            }
        }

        return false;
    }

    public async IAsyncEnumerable<NpgsqlDataReader> ReadExistingRowsFromSnapshot(
        string snapshotName,
        string tableName,
        SnapshotCheckpoint checkpoint,
        int batchSize,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        const string createdAt = "CreatedAt";
        const string eventId = "EventId";
        const string cursorName = "snapshot_cursor";

        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await using var initCursorCommand = new NpgsqlBatch(connection, transaction)
        {
            BatchCommands =
            {
                new($"SET TRANSACTION SNAPSHOT '{snapshotName}';"),
                new($"""
                    DECLARE {cursorName} CURSOR FOR
                        SELECT *
                        FROM "{tableName}"
                        WHERE ("{createdAt}" > $1 OR ("{createdAt}" = $1 AND "{eventId}" > $2))
                        ORDER BY "{createdAt}" ASC, "{eventId}" ASC;
                    """)
                {
                    Parameters = { new() { Value = checkpoint.ConfirmedAt }, new() { Value = checkpoint.ConfirmedId } }
                }
            }
        };
        await initCursorCommand.ExecuteNonQueryAsync(ct);

        await using var fetchCursorCommand = new NpgsqlCommand($"FETCH {batchSize} FROM {cursorName};", connection, transaction);
        await fetchCursorCommand.PrepareAsync(ct);
        try
        {
            while (true)
            {
                await using var reader = await fetchCursorCommand.ExecuteReaderAsync(ct);

                if (!reader.HasRows)
                {
                    yield break;
                }

                while (await reader.ReadAsync(ct))
                {
                    yield return reader;
                }
            }
        }
        finally
        {
            await fetchCursorCommand.UnprepareAsync(ct);
        }
    }

    private async Task UpsertCheckpoints_Private(ICollection<SnapshotCheckpoint> checkpoints, CancellationToken ct = default)
    {
        const int numberOfParametersZeroIndexed = 2;

        if (checkpoints.Count == 0)
        {
            return;
        }

        var values = string.Join(",", checkpoints.Select((_, index) =>
        {
            var number = 1 + index + (numberOfParametersZeroIndexed * index);
            return $"(${number},${number + 1},${number + 2})";
        }));

        await using var command = _dataSource.CreateCommand($"""
            INSERT INTO cdc_snapshot_checkpoint (slot_name, confirmed_at, confirmed_id)
            VALUES {values}
            ON CONFLICT (slot_name) DO UPDATE
            SET confirmed_at = EXCLUDED.confirmed_at, confirmed_id = EXCLUDED.confirmed_id;
            """);

        foreach (var checkpoint in checkpoints)
        {
            command.Parameters.AddWithValue(checkpoint.SlotName);
            command.Parameters.AddWithValue(checkpoint.ConfirmedAt);
            command.Parameters.AddWithValue(checkpoint.ConfirmedId);
        }

        await command.ExecuteNonQueryAsync(ct);
    }
}
