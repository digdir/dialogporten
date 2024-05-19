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
    string TableName,
    string PublicationName = null!,
    string ReplicationSlotName = null!)
{
    public string PublicationName { get; init; } = PublicationName ?? $"{TableName.Trim().ToLowerInvariant()}_insert_publication";
    public string ReplicationSlotName { get; init; } = ReplicationSlotName ?? $"{TableName.Trim().ToLowerInvariant()}_cdc_replication_slot";
}

internal sealed class PostgresOutboxCdcSubscription : ICdcSubscription<OutboxMessage>, IAsyncDisposable
{
    private readonly LogicalReplicationConnection _replicationConnection;
    private readonly NpgsqlDataSource _dataSource;
    private readonly PostgresOutboxCdcSSubscriptionOptions _options;
    private readonly IReplicationDataMapper<OutboxMessage> _mapper;

    private bool _disposed;
    private bool _snapshotConsumed = true;
    private SnapshotCheckpoint _checkpoint = SnapshotCheckpoint.Default;

    public PostgresOutboxCdcSubscription(PostgresOutboxCdcSSubscriptionOptions options, IReplicationDataMapper<OutboxMessage> mapper)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _replicationConnection = new LogicalReplicationConnection(options.ConnectionString);
        _dataSource = NpgsqlDataSource.Create(options.ConnectionString);
    }

    public async IAsyncEnumerable<OutboxMessage> Subscribe([EnumeratorCancellation] CancellationToken ct = default)
    {
        CheckDisposed();
        var (_, tableName, publicationName, slotName) = _options;
        await _replicationConnection.Open(ct);

        await foreach (var outboxMessage in CreateSubscription(ct))
        {
            yield return outboxMessage;
        }

        var slot = new PgOutputReplicationSlot(slotName);
        var replicationOptions = new PgOutputReplicationOptions(publicationName, 1);
        var checkpointSyncCounter = 0;
        await foreach (var message in _replicationConnection.StartReplication(slot, replicationOptions, ct))
        {
            if (message is InsertMessage insertMessage && insertMessage.Relation.RelationName == tableName)
            {
                var outboxMessage = await _mapper.ReadFromReplication(insertMessage, ct);
                yield return outboxMessage;

                _checkpoint = outboxMessage.ToSnapshotCheckpoint();
                // TODO: Should the sync threshold be configurable?
                if (++checkpointSyncCounter >= 1000)
                {
                    await _dataSource.SetSnapshotCheckpoint(slotName, _checkpoint, ct);
                    checkpointSyncCounter = 0;
                }
            }

            // Always call SetReplicationStatus() or assign LastAppliedLsn and LastFlushedLsn individually
            // so that Npgsql can inform the server which WAL files can be removed/recycled.
            _replicationConnection.SetReplicationStatus(message.WalEnd);
            await _replicationConnection.SendStatusUpdate(ct);
        }
    }

    private async IAsyncEnumerable<OutboxMessage> CreateSubscription(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var (_, tableName, publicationName, slotName) = _options;
        await _dataSource.EnsureInsertPublicationForTable(tableName, publicationName, ct);

        if (await _dataSource.ReplicationSlotExists(slotName, ct))
        {
            yield break;
        }

        _snapshotConsumed = false;
        _checkpoint = await _dataSource.GetSnapshotCheckpoint(slotName, ct);
        // Ensure that we have access to the snapshot before creating the replication slot and consuming the snapshot.
        await _dataSource.SetSnapshotCheckpoint(slotName, _checkpoint, ct);

        var replicationSlot = await _replicationConnection.CreatePgOutputReplicationSlot(
            slotName,
            slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
            cancellationToken: ct);

        var checkpointSyncCounter = 0;
        await foreach (var reader in _dataSource.ReadExistingRowsFromSnapshot(replicationSlot.SnapshotName!, tableName, _checkpoint, ct))
        {
            var outboxMessage = await _mapper.ReadFromSnapshot(reader, ct);
            yield return outboxMessage;

            _checkpoint = outboxMessage.ToSnapshotCheckpoint();
            // TODO: Should the sync threshold be configurable?
            if (++checkpointSyncCounter >= 1000)
            {
                await _dataSource.SetSnapshotCheckpoint(slotName, _checkpoint, ct);
                checkpointSyncCounter = 0;
            }
        }

        _snapshotConsumed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (!_snapshotConsumed)
        {
            // The snapshot represents the state of the table at the time the slot was created, and the
            // slot represents all changes that happons from that point on. If we fail to read the
            // snapshot in its entirety, we should start from scratch the next time the subscription
            // is started.
            await _dataSource.DropReplicationSlot(_options.ReplicationSlotName);
        }

        if (_checkpoint != SnapshotCheckpoint.Default)
        {
            await _dataSource.SetSnapshotCheckpoint(_options.ReplicationSlotName, _checkpoint);
        }

        await _replicationConnection.DisposeAsync();
        await _dataSource.DisposeAsync();
        _disposed = true;
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
