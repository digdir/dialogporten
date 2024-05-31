namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;

internal record struct Checkpoint(string SlotName, DateTimeOffset ConfirmedAt, Guid ConfirmedId)
{
    public DateTimeOffset ConfirmedAt { get; init; } = ConfirmedAt.ToUniversalTime();
    public static Checkpoint Default(string slotName) => new(slotName, DateTimeOffset.MinValue, Guid.Empty);
}
