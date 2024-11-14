using Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;

internal sealed class CheckpointSynchronizer : BackgroundService, IAsyncDisposable
{
    private IReadOnlyCollection<Checkpoint> _syncedCheckpoints = new List<Checkpoint>().AsReadOnly();

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly PeriodicTimer _periodicTimer;

    private readonly ICheckpointCache _snapshotCache;
    private readonly ICheckpointRepository _snapshotRepository;
    private readonly ILogger<CheckpointSynchronizer> _logger;
    private readonly IDisposable _optionsChangeSubscription;

    public CheckpointSynchronizer(
        ICheckpointCache cache,
        ICheckpointRepository snapshotRepository,
        ILogger<CheckpointSynchronizer> logger,
        IOptionsMonitor<OutboxCdcSubscriptionOptions> options)
    {
        _snapshotCache = cache ?? throw new ArgumentNullException(nameof(cache));
        _snapshotRepository = snapshotRepository ?? throw new ArgumentNullException(nameof(snapshotRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _optionsChangeSubscription = options?.OnChange(UpdateOptions) ?? throw new ArgumentNullException(nameof(options));
        _periodicTimer = new(options.CurrentValue.CheckpointSynchronizationInterval);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // This method will block the application from starting until
        // snapshots are loaded and permissions are verified

        // TODO: https://github.com/digdir/dialogporten/issues/...
        // Remove when checkpoint migration exists
        // Ensure table exists
        await _snapshotRepository.EnsureCheckpointTableExists(cancellationToken);

        // Load the checkpoints from the database and add it to the cache
        _syncedCheckpoints = await _snapshotRepository.GetCheckpoints(cancellationToken);
        _snapshotCache.UpsertRange(_syncedCheckpoints);

        // Ensure that the application has the necessary permissions to create checkpoints
        if (!await _snapshotRepository.TryUpsertCheckpoints([Checkpoint.Default("cdc_health_check")], cancellationToken))
        {
            throw new InvalidOperationException(
                "Failed to create the default snapshot checkpoint. The application " +
                "may lack appropriate permissions and will shut down.");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Starting checkpoint synchronizer.");
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            if (!await _semaphore.WaitAsync(0, stoppingToken))
            {
                continue;
            }

            try
            {
                if (!await TrySync(stoppingToken))
                {
                    _logger.LogWarning("Failed to sync snapshot checkpoints. Will retry in next iteration.");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public override void Dispose()
    {
        _optionsChangeSubscription.Dispose();
        _periodicTimer.Dispose();
        _semaphore.Dispose();
        base.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync();
        if (!await TrySync())
        {
            _logger.LogError("Failed to sync snapshot checkpoints. The subscriptions may be in an inconsistent state that may result in duplicate messages.");
        }

        Dispose();
    }

    private async Task<bool> TrySync(CancellationToken stoppingToken = default)
    {
        var current = _snapshotCache.GetAll();
        var unsynced = current
            .Except(_syncedCheckpoints)
            .ToList();

        if (unsynced.Count == 0)
        {
            return true;
        }

        _logger.LogDebug("Syncing {UnsyncedCheckpointCount} unsynced checkpoints.", unsynced.Count);

        if (!await _snapshotRepository.TryUpsertCheckpoints(unsynced, stoppingToken))
        {
            return false;
        }

        _syncedCheckpoints = current;
        return true;
    }

    private void UpdateOptions(OutboxCdcSubscriptionOptions options)
    {
        try
        {
            _periodicTimer.Period = options.CheckpointSynchronizationInterval;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An error occurred while updating the options for checkpoint synchronization.");
        }
    }
}
