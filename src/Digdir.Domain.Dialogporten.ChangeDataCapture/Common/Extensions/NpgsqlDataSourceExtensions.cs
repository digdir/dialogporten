using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;

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
        const int batchSize = 1000; // Define a suitable batch size
        const string createdAt = "CreatedAt";
        const string eventId = "EventId";

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await using var command = new NpgsqlCommand($"SET TRANSACTION SNAPSHOT '{snapshotName}';", connection, transaction);
        await command.ExecuteNonQueryAsync(ct);

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            // TODO: Where and order by clause makes this tightly cupled to the outbox table. Fix? 
            // TODO: Set batch size variable? 
            await using var cmd = new NpgsqlCommand(
                $"""
                SELECT *
                FROM "{tableName}"
                WHERE "{createdAt}" > $1 
                    OR ("{createdAt}" = $1 AND "{eventId}" > $2)
                ORDER BY "{createdAt}" ASC, "{eventId}" ASC
                LIMIT $3
                """, connection, transaction);
            cmd.Parameters.AddWithValue(checkpoint.ConfirmedAt);
            cmd.Parameters.AddWithValue(checkpoint.ConfirmedId);
            cmd.Parameters.AddWithValue(batchSize);
            await cmd.PrepareAsync(ct);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                yield return reader;
            }
        }
    }

    public static async IAsyncEnumerable<NpgsqlDataReader> ReadExistingRowsFromSnapshot2(
    this NpgsqlDataSource dataSource,
    string snapshotName,
    string tableName,
    SnapshotCheckpoint checkpoint,
    [EnumeratorCancellation] CancellationToken ct = default)
    {
        const int batchSize = 1000; // Define a suitable batch size
        const string createdAt = "CreatedAt";
        const string eventId = "EventId";

        var (confirmedAt, confirmedId) = checkpoint;
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await using var command = new NpgsqlCommand($"SET TRANSACTION SNAPSHOT '{snapshotName}';", connection, transaction);
        await command.ExecuteNonQueryAsync(ct);

        bool moreRows = true;
        while (moreRows && !ct.IsCancellationRequested)
        {
            await using var cmd = new NpgsqlCommand(
                $"""
                SELECT *
                FROM "{tableName}"
                WHERE ("{createdAt}" > $1 OR ("{createdAt}" = $1 AND "{eventId}" > $2))
                ORDER BY "{createdAt}" ASC, "{eventId}" ASC
                LIMIT {batchSize}
                """, connection, transaction);
            cmd.Parameters.AddWithValue(confirmedAt);
            cmd.Parameters.AddWithValue(confirmedId);
            await cmd.PrepareAsync(ct);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            moreRows = false;

            while (await reader.ReadAsync(ct))
            {
                moreRows = true;
                yield return reader;
                checkpoint.ConfirmedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                checkpoint.ConfirmedId = reader.GetGuid(reader.GetOrdinal("EventId"));
            }
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
