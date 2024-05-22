namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal record struct SnapshotCheckpoint(DateTimeOffset ConfirmedAt, Guid ConfirmedId)
{
    public static readonly SnapshotCheckpoint Default = new(DateTimeOffset.MinValue, Guid.Empty);
}
