using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Azure.Messaging.EventGrid.SystemEvents;
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
    private const int SnapshotSyncThreshold = 1000;
    private readonly LogicalReplicationConnection _replicationConnection;
    private readonly NpgsqlDataSource _dataSource;
    private readonly PostgresOutboxCdcSSubscriptionOptions _options;
    private readonly IReplicationDataMapper<OutboxMessage> _mapper;
    private readonly ILogger<PostgresOutboxCdcSubscription> _logger;

    private bool _disposed;
    private bool _replicationSnapshotConsumed = true;
    private SnapshotCheckpoint _checkpoint = SnapshotCheckpoint.Default;

    public PostgresOutboxCdcSubscription(
        PostgresOutboxCdcSSubscriptionOptions options,
        IReplicationDataMapper<OutboxMessage> mapper,
        ILogger<PostgresOutboxCdcSubscription> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _replicationConnection = new LogicalReplicationConnection(options.ConnectionString);
        _dataSource = NpgsqlDataSource.Create(options.ConnectionString);
    }

    public async IAsyncEnumerable<OutboxMessage> Subscribe([EnumeratorCancellation] CancellationToken ct = default)
    {
        CheckDisposed();
        await _replicationConnection.Open(ct);
        var result = await CreateSubscription(ct);
        if (result is SubscriptionResult.Created created)
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

    private async Task<SubscriptionResult> CreateSubscription(CancellationToken ct)
    {
        var (_, tableName, publicationName, slotName) = _options;
        await _dataSource.EnsureInsertPublicationForTable(tableName, publicationName, ct);
        if (await _dataSource.ReplicationSlotExists(slotName, ct))
        {
            return new SubscriptionResult.Exists();
        }

        // At this point we know that the replication slot does not exist, and we need to create it.
        // We also need to consume every row in the target table from the replication slot
        // transaction snapshot taking into account the snapshot checkpoint.

        _checkpoint = await _dataSource.GetSnapshotCheckpoint(slotName, ct);
        // Ensure that we have access to the snapshot before creating the replication slot and consuming the snapshot.
        await _dataSource.SetSnapshotCheckpoint(slotName, _checkpoint, ct);

        _replicationSnapshotConsumed = false;
        var replicationSlot = await _replicationConnection.CreatePgOutputReplicationSlot(
            slotName,
            slotSnapshotInitMode: LogicalSlotSnapshotInitMode.Export,
            cancellationToken: ct);
        return new SubscriptionResult.Created(replicationSlot);
    }

    private async IAsyncEnumerable<OutboxMessage> ConsumeSnapshot(
        SubscriptionResult.Created created,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var reader in _dataSource
            .ReadExistingRowsFromSnapshot(created.ReplicationSlot.SnapshotName!, _options.TableName, _checkpoint, ct)
            .ForEvery(SnapshotSyncThreshold, SetSnapshotCheckpoint))
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
            .ForEvery(SnapshotSyncThreshold, SetSnapshotCheckpoint))
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
        _dataSource.SetSnapshotCheckpoint(_options.ReplicationSlotName, _checkpoint);

    private async Task SetSnapshotCheckpointWithRetry()
    {
        const int numberOfRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(2);
        using var periodicTimer = new PeriodicTimer(retryDelay);

        for (var attempt = 0; attempt < numberOfRetries && await periodicTimer.WaitForNextTickAsync(); attempt++)
        {
            try
            {
                await SetSnapshotCheckpoint();
                break;
            }
            catch (Exception ex)
            {
                if (attempt + 1 >= numberOfRetries)
                {
                    _logger.LogError(ex, "Failed to set snapshot checkpoint after {NumberOfRetries} attempts.", numberOfRetries);
                }

                _logger.LogWarning(ex, "Failed to set snapshot checkpoint. Retrying in {RetryDelay}.", retryDelay);
            }
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
            await _dataSource.DropReplicationSlot(_options.ReplicationSlotName);
        }

        await SetSnapshotCheckpointWithRetry();
        await _replicationConnection.DisposeAsync();
        await _dataSource.DisposeAsync();
        _disposed = true;
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private abstract record SubscriptionResult
    {
        internal record Created(PgOutputReplicationSlot ReplicationSlot) : SubscriptionResult;
        internal record Exists : SubscriptionResult;
    }
}
