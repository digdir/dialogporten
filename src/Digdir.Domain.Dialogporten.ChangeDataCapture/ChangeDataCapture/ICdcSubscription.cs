using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

internal interface ICdcSubscription<T>
{
    IAsyncEnumerable<T> Subscribe(CancellationToken ct);
}

public record PostgresCdcSSubscriptionOptions(
    string ConnectionString,
    string ReplicationSlotName,
    string PublicationName,
    string TableName,
    IReplicationDataMapper<OutboxMessage> DataMapper
)
{
    internal string ReplicationSlotName { get; init; } = ReplicationSlotName.ToLowerInvariant();
    internal string PublicationName { get; init; } = PublicationName.ToLowerInvariant();
}

internal sealed class PostgresCdcSubscription : ICdcSubscription<OutboxMessage>
{
    private readonly PostgresCdcSSubscriptionOptions _options;

    public PostgresCdcSubscription(PostgresCdcSSubscriptionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async IAsyncEnumerable<OutboxMessage> Subscribe([EnumeratorCancellation] CancellationToken ct = default)
    {
        var (connectionString, slotName, publicationName, tableName, dataMapper) = _options;
        await using var connection = new LogicalReplicationConnection(connectionString);
        await connection.Open(ct);

        await foreach (var reader in CreateSubscription(connection, ct))
        {
            yield return await dataMapper.ReadFromSnapshot(reader, ct);
        }

        var slot = new PgOutputReplicationSlot(slotName);
        var replicationOptions = new PgOutputReplicationOptions(publicationName, 1);
        await foreach (var message in connection.StartReplication(slot, replicationOptions, ct))
        {
            if (message is InsertMessage insertMessage && insertMessage.Relation.RelationName == tableName)
            {
                yield return await dataMapper.ReadFromReplication(insertMessage, ct);
            }

            // Always call SetReplicationStatus() or assign LastAppliedLsn and LastFlushedLsn individually
            // so that Npgsql can inform the server which WAL files can be removed/recycled.
            connection.SetReplicationStatus(message.WalEnd);
            await connection.SendStatusUpdate(ct);
        }
    }

    private async IAsyncEnumerable<NpgsqlDataReader> CreateSubscription(
        LogicalReplicationConnection connection,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var (connectionString, slotName, publicationName, tableName, _) = _options;
        var dataSource = NpgsqlDataSource.Create(connectionString);

        var publicationExists = await dataSource.Exists("pg_publication", "pubname = $1", [publicationName], ct);
        if (!publicationExists)
        {
            await dataSource.Execute($"""CREATE PUBLICATION {publicationName} FOR TABLE "{tableName}" WITH (publish = 'insert', publish_via_partition_root = false);""", ct);
        }

        var replicationSlotExists = await dataSource.Exists("pg_replication_slots", "slot_name = $1", [slotName], ct);
        if (!replicationSlotExists)
        {
            var replicationSlot = await connection.CreatePgOutputReplicationSlot(slotName,
                slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
                cancellationToken: ct);

            await foreach (var reader in dataSource.ReadExistingRowsFromSnapshot(replicationSlot.SnapshotName!, tableName, ct))
            {
                yield return reader;
            }
        }
    }
}
