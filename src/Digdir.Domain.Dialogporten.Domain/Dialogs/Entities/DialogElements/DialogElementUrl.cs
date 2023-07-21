using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features;
using Digdir.Library.Entity.Abstractions.Features.Hierarchy;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

public class DialogElementUrl : IEntity, ISubEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Uri Url { get; set; } = null!;
    public string? MimeType { get; set; }

    public IEntityBase Parent => DialogElement;

    // === Dependent relationships ===
    public DialogElementUrlConsumerType.Enum ConsumerTypeId { get; set; }
    public DialogElementUrlConsumerType ConsumerType { get; set; } = null!;

    public Guid DialogElementId { get; set; }
    public DialogElement DialogElement { get; set; } = null!;
}
