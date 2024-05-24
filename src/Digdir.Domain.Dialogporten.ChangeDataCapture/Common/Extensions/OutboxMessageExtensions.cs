using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Snapshot;
using Digdir.Domain.Dialogporten.Domain.Outboxes;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;

internal static class OutboxMessageExtensions
{
    public static SnapshotCheckpoint ToSnapshotCheckpoint(this OutboxMessage outboxMessage) =>
        new(outboxMessage.CreatedAt, outboxMessage.EventId);
}
