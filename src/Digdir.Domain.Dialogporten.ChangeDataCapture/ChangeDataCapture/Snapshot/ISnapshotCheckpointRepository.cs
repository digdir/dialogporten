namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal interface ISnapshotCheckpointRepository
{
    Task<SnapshotCheckpoint> Get(string slotName, CancellationToken ct = default);
    Task Upsert(string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default);
    Task<bool> TryUpsertWithRetry(string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default);
}
