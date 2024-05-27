namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;

internal record struct SnapshotCheckpoint(DateTimeOffset ConfirmedAt, Guid ConfirmedId)
{
    public DateTimeOffset ConfirmedAt { get; init; } = ConfirmedAt.ToUniversalTime();
    public static readonly SnapshotCheckpoint Default = new(DateTimeOffset.MinValue, Guid.Empty);
}
