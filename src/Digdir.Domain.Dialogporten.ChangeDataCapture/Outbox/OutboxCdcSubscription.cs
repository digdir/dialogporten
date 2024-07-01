using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Mappers;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscriptions;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Microsoft.Extensions.Options;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;

internal sealed class OutboxCdcSubscription : ICdcSubscription<OutboxMessage>, IAsyncDisposable
{
    private bool _disposed;
    private bool _replicationSnapshotConsumed = true;

    private readonly LogicalReplicationConnection _replicationConnection;

    private readonly OutboxCdcSubscriptionOptions _startupOptions;
    private readonly IOptionsMonitor<OutboxCdcSubscriptionOptions> _options;
    private readonly IReplicationMapper<OutboxMessage> _mapper;
    private readonly IOutboxReaderRepository _outboxRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICheckpointCache _checkpointCache;
    private readonly ILogger<OutboxCdcSubscription> _logger;

    public OutboxCdcSubscription(
        ILogger<OutboxCdcSubscription> logger,
        IOptionsMonitor<OutboxCdcSubscriptionOptions> options,
        IReplicationMapper<OutboxMessage> mapper,
        IOutboxReaderRepository outboxRepository,
        ISubscriptionRepository subscriptionRepository,
        ICheckpointCache checkpointCache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _startupOptions = options.CurrentValue;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _checkpointCache = checkpointCache ?? throw new ArgumentNullException(nameof(checkpointCache));

        _replicationConnection = new LogicalReplicationConnection(_options.CurrentValue.ConnectionString);
    }

    public async IAsyncEnumerable<IReadOnlyCollection<OutboxMessage>> Subscribe([EnumeratorCancellation] CancellationToken ct = default)
    {
        /* TODO:
         * - Opprett Checkpoint tabell i infrastruktur
         * - Opprett OutboxMessage tabell i infrastruktur
         * - Opprett OutboxMessageConsumer tabell i infrastruktur
         */

        CheckDisposed();
        await _replicationConnection.Open(ct);
        if (await CreateSubscription(ct) is SubscriptionResult.Created created)
        {
            await foreach (var outboxMessages in ConsumeSnapshot(created, ct))
            {
                yield return outboxMessages;
            }
        }

        await foreach (var outboxMessages in ConsumeReplicationSlot(ct))
        {
            yield return outboxMessages;
        }
    }

    private async Task<SubscriptionResult> CreateSubscription(CancellationToken ct)
    {
        _logger.LogDebug("Creating subscription for table '{TableName}' with publication '{PublicationName}' and replication slot '{ReplicationSlot}'.",
            _startupOptions.TableName, _startupOptions.PublicationName, _startupOptions.ReplicationSlotName);

        await _subscriptionRepository.EnsureInsertPublicationForTable(_startupOptions.TableName, _startupOptions.PublicationName, ct);
        if (await _subscriptionRepository.ReplicationSlotExists(_startupOptions.ReplicationSlotName, ct))
        {
            _logger.LogDebug("Subscription already exists for replication slot '{ReplicationSlot}'.", _startupOptions.ReplicationSlotName);
            return new SubscriptionResult.Exists();
        }

        // At this point we know that the replication slot does not exist, and we need to create it.
        // We also need to consume every row in the target table from the replication slot
        // transaction snapshot, taking into account the snapshot checkpoint.

        _replicationSnapshotConsumed = false;
        var replicationSlot = await _replicationConnection.CreatePgOutputReplicationSlot(
            _startupOptions.ReplicationSlotName,
            slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
            cancellationToken: ct);

        _logger.LogDebug("Replication slot '{ReplicationSlot}' created.", _startupOptions.ReplicationSlotName);
        return new SubscriptionResult.Created(replicationSlot);
    }

    private async IAsyncEnumerable<IReadOnlyCollection<OutboxMessage>> ConsumeSnapshot(
        SubscriptionResult.Created created,
        [EnumeratorCancellation] CancellationToken ct)
    {
        _logger.LogDebug("Consuming snapshot for replication slot '{ReplicationSlot}'.", _startupOptions.ReplicationSlotName);
        var checkpoint = _checkpointCache.GetOrDefault(_startupOptions.ReplicationSlotName);
        var snapshotName = created.ReplicationSlot.SnapshotName!;
        var outboxMessages = new OutboxMessage[1];
        await foreach (var reader in _outboxRepository.ReadFromCheckpoint(checkpoint, snapshotName, ct))
        {
            outboxMessages[0] = await _mapper.ReadFromSnapshot(reader, ct);
            yield return outboxMessages;
            _checkpointCache.Upsert(outboxMessages[0].ToCheckpoint(_startupOptions.ReplicationSlotName));
        }

        _replicationSnapshotConsumed = true;
        _logger.LogDebug("Snapshot consumed for replication slot '{ReplicationSlot}'.", _startupOptions.ReplicationSlotName);
    }

    private async IAsyncEnumerable<IReadOnlyCollection<OutboxMessage>> ConsumeReplicationSlot(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var options = _options.CurrentValue;
        _logger.LogDebug("Consuming replication slot '{ReplicationSlot}'.", _startupOptions.ReplicationSlotName);
        var slot = new PgOutputReplicationSlot(_startupOptions.ReplicationSlotName);
        var replicationOptions = new PgOutputReplicationOptions(_startupOptions.PublicationName, 1);
        var transactionBatch = new List<OutboxMessage>();
        await foreach (var message in _replicationConnection.StartReplication(slot, replicationOptions, ct))
        {
            switch (message)
            {
                case InsertMessage insertMessage when insertMessage.Relation.RelationName == _startupOptions.TableName:
                    transactionBatch.Add(await _mapper.ReadFromReplication(insertMessage, ct));
                    break;
                case CommitMessage when transactionBatch.Count > 0:
                    yield return transactionBatch;
                    _checkpointCache.Upsert(transactionBatch
                        .MaxBy(x => x.CreatedAt)!
                        .ToCheckpoint(_startupOptions.ReplicationSlotName, _options.CurrentValue.ReplicationCheckpointTimeSkew));
                    transactionBatch.Clear();
                    break;
                default: break;
            }

            await _replicationConnection.AcknowledgeWalMessage(message, ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (!_replicationSnapshotConsumed)
        {
            // The snapshot represents the state of the table at the time the slot was created, and the
            // slot represents all changes that happons from that point on. If we fail to read the
            // snapshot in its entirety, we should start from scratch the next time the subscription
            // is started.
            _logger.LogDebug("Replication slot '{ReplicationSlot}' was not fully consumed, dropping slot.", _startupOptions.ReplicationSlotName);
            await _subscriptionRepository.DropReplicationSlot(_startupOptions.ReplicationSlotName);
        }

        await _replicationConnection.DisposeAsync();
        _disposed = true;
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private abstract record SubscriptionResult
    {
        internal sealed record Created(PgOutputReplicationSlot ReplicationSlot) : SubscriptionResult;
        internal sealed record Exists : SubscriptionResult;
    }
}
