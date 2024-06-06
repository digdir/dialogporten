using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;
using Digdir.Domain.Dialogporten.Domain.Outboxes;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;

internal static class OutboxMessageExtensions
{
    public static Checkpoint ToCheckpoint(this OutboxMessage outboxMessage, string slotName, TimeSpan timeSkew = default) =>
        new(slotName, outboxMessage.CreatedAt + timeSkew, outboxMessage.EventId);
}
