using System.Data;
using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;
internal interface IOutboxReaderRepository
{
    IAsyncEnumerable<NpgsqlDataReader> ReadFromCheckpoint(
        Checkpoint checkpoint,
        string? snapshotName = null,
        CancellationToken ct = default);
}

internal sealed class OutboxReaderRepository : IOutboxReaderRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly OutboxCdcSubscriptionOptions _options;

    public OutboxReaderRepository(NpgsqlDataSource dataSource, IOptions<OutboxCdcSubscriptionOptions> options)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _options = options.Value;
    }

    public async IAsyncEnumerable<NpgsqlDataReader> ReadFromCheckpoint(
        Checkpoint checkpoint,
        string? snapshotName = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        const string createdAt = nameof(OutboxMessage.CreatedAt);
        const string eventId = nameof(OutboxMessage.EventId);
        const string cursorName = "outbox_snapshot_cursor";
        var tableName = _options.TableName;
        var batchSize = _options.SnapshotBatchSize;

        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await using var initCursorCommand = new NpgsqlBatch(connection, transaction);

        if (!string.IsNullOrWhiteSpace(snapshotName))
        {
            initCursorCommand.BatchCommands.Add(new($"SET TRANSACTION SNAPSHOT '{snapshotName}';"));
        }

        initCursorCommand.BatchCommands.Add(new($"""
            DECLARE {cursorName} CURSOR FOR
                SELECT *
                FROM "{tableName}"
                WHERE ("{createdAt}" > $1 OR ("{createdAt}" = $1 AND "{eventId}" > $2))
                ORDER BY "{createdAt}" ASC, "{eventId}" ASC;
            """)
        {
            Parameters =
            {
                new() { Value = checkpoint.ConfirmedAt },
                new() { Value = checkpoint.ConfirmedId }
            }
        });

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
}
