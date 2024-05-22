using Npgsql;
using System.Data;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal sealed class SnapshotCheckpointRepository : ISnapshotCheckpointRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<SnapshotCheckpointRepository> _logger;

    public SnapshotCheckpointRepository(
        NpgsqlDataSource dataSource,
        ILogger<SnapshotCheckpointRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SnapshotCheckpoint> Get(string slotName, CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand("SELECT confirmed_at, confirmed_id FROM cdc_snapshot_checkpoint WHERE slot_name = $1");
        command.Parameters.AddWithValue(slotName);
        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return SnapshotCheckpoint.Default;

        var confirmedAt = await reader.GetFieldValueAsync<DateTimeOffset>("confirmed_date", ct);
        var confirmedId = await reader.GetFieldValueAsync<Guid>("confirmed_id", ct);
        return new(confirmedAt, confirmedId);
    }

    public async Task Upsert(string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand(
            $"""
            INSERT INTO cdc_snapshot_checkpoint (slot_name, confirmed_at, confirmed_id)
            VALUES ($1, $2, $3)
            ON CONFLICT (slot_name) DO UPDATE
            SET confirmed_at = EXCLUDED.confirmed_at, confirmed_id = EXCLUDED.confirmed_id;
            """);
        command.Parameters.AddWithValue(slotName);
        command.Parameters.AddWithValue(checkpoint.ConfirmedAt);
        command.Parameters.AddWithValue(checkpoint.ConfirmedId);
        await command.PrepareAsync(ct);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> TryUpsertWithRetry(string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default)
    {
        const int numberOfRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(2);
        using var periodicTimer = new PeriodicTimer(retryDelay);

        for (var attempt = 0; attempt < numberOfRetries; attempt++)
        {
            await periodicTimer.WaitForNextTickAsync(ct);

            try
            {
                await Upsert(slotName, checkpoint, ct);
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
}
