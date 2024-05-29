namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal record struct SnapshotCheckpoint(string SlotName, DateTimeOffset ConfirmedAt, Guid ConfirmedId)
{
    public DateTimeOffset ConfirmedAt { get; init; } = ConfirmedAt.ToUniversalTime();
    public static SnapshotCheckpoint Default(string slotName) => new(slotName, DateTimeOffset.MinValue, Guid.Empty);
}
