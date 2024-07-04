namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;

internal interface ICheckpointCache
{
    Checkpoint GetOrDefault(string slotName);
    IReadOnlyCollection<Checkpoint> GetAll();
    void Upsert(Checkpoint checkpoint);
    void UpsertRange(IEnumerable<Checkpoint> checkpoints);
}

internal sealed class CheckpointCache : ICheckpointCache
{
    private readonly Dictionary<string, Checkpoint> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public Checkpoint GetOrDefault(string slotName) =>
        _cache.TryGetValue(slotName, out var checkpoint)
            ? checkpoint
            : Checkpoint.Default(slotName);

    public void Upsert(Checkpoint checkpoint) =>
        _cache[checkpoint.SlotName] = checkpoint;

    public void UpsertRange(IEnumerable<Checkpoint> checkpoints)
    {
        foreach (var checkpoint in checkpoints)
        {
            Upsert(checkpoint);
        }
    }

    public IReadOnlyCollection<Checkpoint> GetAll() =>
        _cache.Values
            .ToList()
            .AsReadOnly();
}
