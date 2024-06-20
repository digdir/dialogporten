using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;

public class DialogAttachmentUrl : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? MediaType { get; set; } = null!;
    public Uri Url { get; set; } = null!;

    // === Dependent relationships ===
    public DialogAttachmentUrlConsumerType.Values ConsumerTypeId { get; set; }
    public DialogAttachmentUrlConsumerType ConsumerType { get; set; } = null!;

    public Guid DialogAttachmentId { get; set; }
    public DialogAttachment DialogAttachment { get; set; } = null!;
}
