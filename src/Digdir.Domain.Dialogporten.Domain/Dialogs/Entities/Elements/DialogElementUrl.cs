using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

public class DialogElementUrl : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Uri Url { get; set; } = null!;
    public string? MimeType { get; set; }

    // === Dependent relationships ===
    public DialogElementUrlConsumerType.Values ConsumerTypeId { get; set; }
    public DialogElementUrlConsumerType ConsumerType { get; set; } = null!;

    public Guid DialogElementId { get; set; }
    public DialogElement DialogElement { get; set; } = null!;
}
