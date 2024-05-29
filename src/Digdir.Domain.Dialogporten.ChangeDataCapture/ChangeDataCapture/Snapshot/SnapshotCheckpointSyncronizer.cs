using System.Collections.Concurrent;
using Npgsql.Replication;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal sealed class SnapshotCheckpointSyncronizer : BackgroundService, IAsyncDisposable
{
    private IReadOnlyCollection<SnapshotCheckpoint> _syncedCheckpoints = new List<SnapshotCheckpoint>().AsReadOnly();

    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ISnapshotCheckpointCache _snapshotCache;
    private readonly ISnapshotRepository _snapshotRepository;
    private readonly ILogger<SnapshotCheckpointSyncronizer> _logger;

    public SnapshotCheckpointSyncronizer(
        ISnapshotCheckpointCache cache,
        ISnapshotRepository snapshotRepository,
        ILogger<SnapshotCheckpointSyncronizer> logger)
    {
        _snapshotCache = cache ?? throw new ArgumentNullException(nameof(cache));
        _snapshotRepository = snapshotRepository ?? throw new ArgumentNullException(nameof(snapshotRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Ensure table exists
        await _snapshotRepository.EnsureSnapshotCheckpointTableExists(cancellationToken);

        // Load the checkpoints from the database
        _snapshotCache.AddRange(_syncedCheckpoints = await _snapshotRepository.GetCheckpoints(cancellationToken));

        // Ensure that the application has the necessary permissions to create checkpoints
        if (!await _snapshotRepository.TryUpsertCheckpoints([SnapshotCheckpoint.Default("cdc_health_check")], cancellationToken))
        {
            throw new InvalidOperationException(
                "Failed to create the default snapshot checkpoint. The application " +
                "may lack appropriate permissions and will shut down.");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

        if (!await _snapshotRepository.TryUpsertCheckpoints(unsynced, stoppingToken))
        {
            return false;
        }

        _syncedCheckpoints = current;
        return true;
    }
}
