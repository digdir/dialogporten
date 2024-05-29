namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal interface ISnapshotCheckpointCache
{
    SnapshotCheckpoint Get(string slotName);
    IReadOnlyCollection<SnapshotCheckpoint> GetAll();
    void Add(SnapshotCheckpoint checkpoint);
    void AddRange(IEnumerable<SnapshotCheckpoint> checkpoints);
}

internal sealed class SnapshotCheckpointCache : ISnapshotCheckpointCache
{
    private readonly Dictionary<string, SnapshotCheckpoint> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public SnapshotCheckpoint Get(string slotName) =>
        _cache.TryGetValue(slotName, out var checkpoint)
            ? checkpoint
            : SnapshotCheckpoint.Default(slotName);

    public void Add(SnapshotCheckpoint checkpoint) =>
        _cache[checkpoint.SlotName] = checkpoint;

    public void AddRange(IEnumerable<SnapshotCheckpoint> checkpoints)
    {
        foreach (var checkpoint in checkpoints)
        {
            Add(checkpoint);
        }
    }

    public IReadOnlyCollection<SnapshotCheckpoint> GetAll() =>
        _cache.Values
            .ToList()
            .AsReadOnly();
}
