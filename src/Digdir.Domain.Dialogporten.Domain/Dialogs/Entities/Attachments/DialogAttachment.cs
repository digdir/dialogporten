using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;

public class DialogAttachment : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    // === Principal relationships ===
    [AggregateChild]
    public AttachmentDisplayName? DisplayName { get; set; }

    [AggregateChild]
    public List<DialogAttachmentUrl> Urls { get; set; } = [];
}

public class AttachmentDisplayName : LocalizationSet
{
    public Guid AttachmentId { get; set; }
    public DialogAttachment Attachment { get; set; } = null!;
}
