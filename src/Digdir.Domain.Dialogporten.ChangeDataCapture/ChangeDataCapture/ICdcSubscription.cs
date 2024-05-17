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

public record PostgresOutboxCdcSSubscriptionOptions(
    string ConnectionString,
    string ReplicationSlotName,
    string PublicationName,
    string TableName
)
{
    internal string ReplicationSlotName { get; init; } = ReplicationSlotName.ToLowerInvariant();
    internal string PublicationName { get; init; } = PublicationName.ToLowerInvariant();
}

internal sealed class PostgresOutboxCdcSubscription : ICdcSubscription<OutboxMessage>
{
    private readonly PostgresOutboxCdcSSubscriptionOptions _options;
    private readonly IReplicationDataMapper<OutboxMessage> _mapper;

    public PostgresOutboxCdcSubscription(PostgresOutboxCdcSSubscriptionOptions options, IReplicationDataMapper<OutboxMessage> mapper)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async IAsyncEnumerable<OutboxMessage> Subscribe([EnumeratorCancellation] CancellationToken ct = default)
    {
        var (connectionString, slotName, publicationName, tableName) = _options;
        await using var connection = new LogicalReplicationConnection(connectionString);
        await connection.Open(ct);

        await foreach (var reader in CreateSubscription(connection, ct))
        {
            yield return await _mapper.ReadFromSnapshot(reader, ct);
        }

        var slot = new PgOutputReplicationSlot(slotName);
        var replicationOptions = new PgOutputReplicationOptions(publicationName, 1);
        await foreach (var message in connection.StartReplication(slot, replicationOptions, ct))
        {
            if (message is InsertMessage insertMessage && insertMessage.Relation.RelationName == tableName)
            {
                yield return await _mapper.ReadFromReplication(insertMessage, ct);
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
        var (connectionString, slotName, _, tableName) = _options;
        var dataSource = NpgsqlDataSource.Create(connectionString);
        await dataSource.EnsureInsertPublicationForTable(tableName, ct);
        if (!await dataSource.ReplicationSlotExists(slotName, ct))
        {
            try
            {
                var replicationSlot = await connection.CreatePgOutputReplicationSlot(slotName,
                    slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
                    cancellationToken: ct);

                await foreach (var reader in dataSource.ReadExistingRowsFromSnapshot(replicationSlot.SnapshotName!, tableName, ct))
                {
                    yield return reader;
                }
            }
            catch (Exception)
            {
                // The snapshot represents the state of the table at the time the slot was created, and the
                // slot represents all changes that happons from that point on. If we fail to read the
                // snapshot in its entirety, we should start from scratch the next time the subscription
                // is started.
                await dataSource.DropReplicationSlot(slotName, ct: CancellationToken.None);
                throw;
            }
        }
    }
}
