using System.Runtime.CompilerServices;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

internal interface ISubscription
{
    IAsyncEnumerable<string> Subscribe(SubscriptionOptions options, CancellationToken ct);
}

public record SubscriptionOptions(
    string ConnectionString,
    string ReplicationSlotName,
    string PublicationName,
    string TableName,
    IJsonReplicationDataMapper DataMapper
)
{
    internal string ReplicationSlotName { get; init; } = ReplicationSlotName.ToLower();
    internal string PublicationName { get; init; } = PublicationName.ToLower();
}

public class Subscription : ISubscription
{
    public async IAsyncEnumerable<string> Subscribe(
        SubscriptionOptions options,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var (connectionString, slotName, publicationName, tableName, dataMapper) = options;
        await using var connection = new LogicalReplicationConnection(connectionString);
        await connection.Open(ct);

        await foreach (var reader in CreateSubscription(options, connection, ct))
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

    private static async IAsyncEnumerable<NpgsqlDataReader> CreateSubscription(
        SubscriptionOptions options,
        LogicalReplicationConnection connection,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var (connectionString, slotName, publicationName, tableName, _) = options;
        var dataSource = NpgsqlDataSource.Create(connectionString);

        var publicationExists = await dataSource.Exists("pg_publication", "pubname = $1", new object[] { publicationName }, ct);
        if (!publicationExists)
        {
            await dataSource.Execute($"""CREATE PUBLICATION {publicationName} FOR TABLE "{tableName}" WITH (publish = 'insert', publish_via_partition_root = false);""", ct);
        }

        var replicationSlotExists = await dataSource.Exists("pg_replication_slots", "slot_name = $1", new object[] { slotName }, ct);
        if (!replicationSlotExists)
        {
            var replicationSlot = await connection.CreatePgOutputReplicationSlot(slotName,
                slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
                cancellationToken: ct);

            // Get the records before creating a subscription
            // TODO: What if some of these fail? 
            // TODO: Do we even need them? 
            await foreach (var reader in dataSource.ReadExistingRowsFromSnapshot(replicationSlot.SnapshotName!, tableName, ct))
            {
                yield return reader;
            }
        }
    }
}
