using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

internal static class NpgsqlDataSourceExtensions
{
    public static async Task EnsureInsertPublicationForTable(
        this NpgsqlDataSource dataSource,
        string tableName,
        string publicationName,
        CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand(
            $"""
            DO
            $do$
            BEGIN
                IF NOT EXISTS(
                    SELECT 1 
                    FROM pg_publication 
                    WHERE pubname = '{publicationName}') 
                THEN
                    CREATE PUBLICATION {publicationName} 
                    FOR TABLE "{tableName}" 
                    WITH (publish = 'insert', publish_via_partition_root = false);
                END IF;
            END
            $do$;
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public static async Task<SnapshotCheckpoint> GetSnapshotCheckpoint(this NpgsqlDataSource dataSource, string slotName, CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT confirmed_at, confirmed_id FROM cdc_snapshot_checkpoint WHERE slot_name = $1");
        command.Parameters.AddWithValue(slotName);
        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
        {
            return SnapshotCheckpoint.Default;
        }

        var confirmedAt = await reader.GetFieldValueAsync<DateTimeOffset>("confirmed_date", ct);
        var confirmedId = await reader.GetFieldValueAsync<Guid>("confirmed_id", ct);
        return new(confirmedAt, confirmedId);
    }

    public static async Task SetSnapshotCheckpoint(this NpgsqlDataSource dataSource, string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand(
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

    public static async Task<bool> ReplicationSlotExists(
        this NpgsqlDataSource dataSource,
        string slotName,
        CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT EXISTS(SELECT 1 FROM pg_replication_slots WHERE slot_name = $1)");
        command.Parameters.AddWithValue(slotName);
        return (await command.ExecuteScalarAsync(ct) as bool?) == true;
    }

    public static async Task DropReplicationSlot(
        this NpgsqlDataSource dataSource,
        string slotName,
        CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand(
            $"""
            SELECT pg_drop_replication_slot('{slotName}') 
            FROM pg_replication_slots WHERE slot_name = '{slotName}';
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public static async IAsyncEnumerable<NpgsqlDataReader> ReadExistingRowsFromSnapshot(
        this NpgsqlDataSource dataSource,
        string snapshotName,
        string tableName,
        SnapshotCheckpoint checkpoint,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await using var command = new NpgsqlCommand($"SET TRANSACTION SNAPSHOT '{snapshotName}';", connection, transaction);
        await command.ExecuteNonQueryAsync(ct);

        // TODO: Where and order by clause makes this tightly cupled to the outbox table. Fix? 
        // TODO: Set batch size variable? 
        await using var cmd = new NpgsqlCommand(
            $"""
            SELECT *
            FROM "{tableName}"
            WHERE "CreatedAt" > $1
                OR ("CreatedAt" = $1 AND "EventId" > $2)
            ORDER BY "CreatedAt" ASC, "EventId" ASC
            """, connection, transaction);
        cmd.Parameters.AddWithValue(checkpoint.ConfirmedAt);
        cmd.Parameters.AddWithValue(checkpoint.ConfirmedId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            yield return reader;
        }
    }

    public static SnapshotCheckpoint ToSnapshotCheckpoint(this OutboxMessage outboxMessage) =>
        new(outboxMessage.CreatedAt, outboxMessage.EventId);

    public static bool IsInsertInto(
        this PgOutputReplicationMessage message,
        string tableName,
        [NotNullWhen(true)] out InsertMessage? insertMessage)
    {
        insertMessage = message as InsertMessage;
        return insertMessage is not null && insertMessage.Relation.RelationName == tableName;
    }

    public static async Task AcknowledgeWalMessage(
        this LogicalReplicationConnection replicationConnection,
        PgOutputReplicationMessage message,
        CancellationToken ct = default)
    {
        // Always call SetReplicationStatus() or assign LastAppliedLsn and LastFlushedLsn individually
        // so that Npgsql can inform the server which WAL files can be removed/recycled.
        replicationConnection.SetReplicationStatus(message.WalEnd);
        await replicationConnection.SendStatusUpdate(ct);
    }

    public static async IAsyncEnumerable<TSource> ForEvery<TSource>(
        this IAsyncEnumerable<TSource> source,
        int threshold,
        Func<TSource, Task> action)
    {
        var counter = 0;
        await foreach (var item in source)
        {
            yield return item;

            if (++counter >= threshold)
            {
                await action(item);
                counter = 0;
            }
        }
    }
}
