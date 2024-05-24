using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.ReplicationMapper;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscription;

internal sealed class PostgresOutboxCdcSubscription : ICdcSubscription<OutboxMessage>, IAsyncDisposable
{
    private bool _disposed;
    private bool _replicationSnapshotConsumed = true;
    private SnapshotCheckpoint _checkpoint = SnapshotCheckpoint.Default;

    private readonly LogicalReplicationConnection _replicationConnection;

    private readonly PostgresOutboxCdcSSubscriptionOptions _options;
    private readonly IReplicationDataMapper<OutboxMessage> _mapper;
    private readonly ISnapshotRepository _snapshotRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<PostgresOutboxCdcSubscription> _logger;

    public PostgresOutboxCdcSubscription(
        IOptions<PostgresOutboxCdcSSubscriptionOptions> options,
        IReplicationDataMapper<OutboxMessage> mapper,
        ISnapshotRepository snapshotRepository,
        ISubscriptionRepository subscriptionRepository,
        ILogger<PostgresOutboxCdcSubscription> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _snapshotRepository = snapshotRepository ?? throw new ArgumentNullException(nameof(snapshotRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _replicationConnection = new LogicalReplicationConnection(_options.ConnectionString);
    }

    public async IAsyncEnumerable<OutboxMessage> Subscribe([EnumeratorCancellation] CancellationToken ct = default)
    {
        /* TODO:
         * - Opprett SnapshotCheckpoint tabell i infrastruktur
         * - Opprett OutboxMessage tabell i infrastruktur
         * - Opprett OutboxMessageConsumer tabell i infrastruktur
         * - Lesing av tabllen fra snapshot er sterkt koplet til OutboxMessage. Skal jeg fikse det?
         * - Gjør batch størrelse av snapshot lesing konfigurerbar
         */

        CheckDisposed();
        await _replicationConnection.Open(ct);
        await InitSnapshotCheckpoint(ct);
        if (await CreateSubscription(ct) is SubscriptionResult.Created created)
        {
            await foreach (var outboxMessage in ConsumeSnapshot(created, ct))
            {
                yield return outboxMessage;
            }
        }

        await foreach (var outboxMessage in ConsumeReplicationSlot(ct))
        {
            yield return outboxMessage;
        }
    }

    private async Task InitSnapshotCheckpoint(CancellationToken ct)
    {
        _checkpoint = await _snapshotRepository.GetCheckpoint(_options.ReplicationSlotName, ct);
        // Ensure that we have access to the snapshot before creating the replication slot and consuming the snapshot.
        await _snapshotRepository.UpsertCheckpoint(_options.ReplicationSlotName, _checkpoint, ct);
    }

    private async Task<SubscriptionResult> CreateSubscription(CancellationToken ct)
    {
        await _subscriptionRepository.EnsureInsertPublicationForTable(_options.TableName, _options.PublicationName, ct);
        if (await _subscriptionRepository.ReplicationSlotExists(_options.ReplicationSlotName, ct))
        {
            return new SubscriptionResult.Exists();
        }

        // At this point we know that the replication slot does not exist, and we need to create it.
        // We also need to consume every row in the target table from the replication slot
        // transaction snapshot, taking into account the snapshot checkpoint.

        _replicationSnapshotConsumed = false;
        var replicationSlot = await _replicationConnection.CreatePgOutputReplicationSlot(
            _options.ReplicationSlotName,
            slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
            cancellationToken: ct);
        return new SubscriptionResult.Created(replicationSlot);
    }

    private async IAsyncEnumerable<OutboxMessage> ConsumeSnapshot(
        SubscriptionResult.Created created,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var reader in _snapshotRepository
            .ReadExistingRowsFromSnapshot(created.ReplicationSlot.SnapshotName!, _options.TableName, _checkpoint, ct)
            .ForEvery(_options.SnapshotSyncThreshold, SetSnapshotCheckpoint))
        {
            var outboxMessage = await _mapper.ReadFromSnapshot(reader, ct);
            yield return outboxMessage;
            _checkpoint = outboxMessage.ToSnapshotCheckpoint();
        }

        _replicationSnapshotConsumed = true;
        await SetSnapshotCheckpoint();
    }

    private async IAsyncEnumerable<OutboxMessage> ConsumeReplicationSlot(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var slot = new PgOutputReplicationSlot(_options.ReplicationSlotName);
        var replicationOptions = new PgOutputReplicationOptions(_options.PublicationName, 1);
        await foreach (var message in _replicationConnection
            .StartReplication(slot, replicationOptions, ct)
            .ForEvery(_options.SnapshotSyncThreshold, SetSnapshotCheckpoint))
        {
            if (!message.IsInsertInto(_options.TableName, out var insertMessage))
            {
                await _replicationConnection.AcknowledgeWalMessage(message, ct);
                continue;
            }

            var outboxMessage = await _mapper.ReadFromReplication(insertMessage, ct);
            yield return outboxMessage;
            _checkpoint = outboxMessage.ToSnapshotCheckpoint();
            await _replicationConnection.AcknowledgeWalMessage(message, ct);
        }
    }

    private Task SetSnapshotCheckpoint(object? _ = null) =>
        _snapshotRepository.UpsertCheckpoint(_options.ReplicationSlotName, _checkpoint);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (!await _snapshotRepository.TryUpsertCheckpointWithRetry(_options.ReplicationSlotName, _checkpoint))
        {
            _logger.LogError("Failed to set snapshot checkpoint. The subscription may be in an inconsistent state that may result in duplicate messages.");
        }

        if (!_replicationSnapshotConsumed)
        {
            // The snapshot represents the state of the table at the time the slot was created, and the
            // slot represents all changes that happons from that point on. If we fail to read the
            // snapshot in its entirety, we should start from scratch the next time the subscription
            // is started.
            await _subscriptionRepository.DropReplicationSlot(_options.ReplicationSlotName);
        }

        await _replicationConnection.DisposeAsync();
        _disposed = true;
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private abstract record SubscriptionResult
    {
        internal record Created(PgOutputReplicationSlot ReplicationSlot) : SubscriptionResult;
        internal record Exists : SubscriptionResult;
    }
}
