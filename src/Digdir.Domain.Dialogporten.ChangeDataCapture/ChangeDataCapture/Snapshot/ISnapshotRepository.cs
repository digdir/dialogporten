using System.Runtime.CompilerServices;
using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal interface ISnapshotRepository
{
    Task<SnapshotCheckpoint> GetCheckpoint(string slotName, CancellationToken ct = default);
    Task UpsertCheckpoint(string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default);
    Task<bool> TryUpsertCheckpointWithRetry(string slotName, SnapshotCheckpoint checkpoint, CancellationToken ct = default);
    IAsyncEnumerable<NpgsqlDataReader> ReadExistingRowsFromSnapshot(string snapshotName, string tableName, SnapshotCheckpoint checkpoint, CancellationToken ct = default);
    Task EnsureSnapshotCheckpointTableExists(CancellationToken ct = default);
}
