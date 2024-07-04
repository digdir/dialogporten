using System.Data;
using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;

internal interface ICheckpointRepository
{
    Task EnsureCheckpointTableExists(CancellationToken ct = default);
    Task<List<Checkpoint>> GetCheckpoints(CancellationToken ct = default);
    Task<bool> TryUpsertCheckpoints(ICollection<Checkpoint> checkpoints, CancellationToken ct = default);
}

internal sealed class CheckpointRepository : ICheckpointRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<CheckpointRepository> _logger;

    public CheckpointRepository(
        NpgsqlDataSource dataSource,
        ILogger<CheckpointRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureCheckpointTableExists(CancellationToken ct = default)
    {
        // TODO: Dette burde ligge sammen med OutboxMessage opprettelsen i infrastruktur via EF
        await using var command = _dataSource.CreateCommand(
            $"""
            CREATE TABLE IF NOT EXISTS cdc_checkpoint (
              slot_name VARCHAR(255) PRIMARY KEY,
              confirmed_at timestamp with time zone,
              confirmed_id UUID
            );
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<List<Checkpoint>> GetCheckpoints(CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand("SELECT slot_name, confirmed_at, confirmed_id FROM cdc_checkpoint");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<Checkpoint>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(new(
                await reader.GetFieldValueAsync<string>("slot_name", ct),
                await reader.GetFieldValueAsync<DateTimeOffset>("confirmed_at", ct),
                await reader.GetFieldValueAsync<Guid>("confirmed_id", ct)));
        }

        return result;
    }

    public async Task<bool> TryUpsertCheckpoints(ICollection<Checkpoint> checkpoints, CancellationToken ct = default)
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
                await UpsertCheckpoints_Internal(checkpoints, ct);
                return true;
            }
            catch (Exception ex)
            {
                if (attempt + 1 < numberOfRetries)
                {
                    _logger.LogWarning(ex, "Failed to set checkpoint. Retrying in {RetryDelay}.", retryDelay);
                }
            }
        }

        return false;
    }

    private async Task UpsertCheckpoints_Internal(ICollection<Checkpoint> checkpoints, CancellationToken ct = default)
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
            INSERT INTO cdc_checkpoint (slot_name, confirmed_at, confirmed_id)
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
