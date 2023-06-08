using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements.DialogElementUrls;

public class DialogElementUrl : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DialogElementUrlConsumerType.Enum ConsumerTypeId { get; set; }
    public Uri Url { get; set; } = null!;
    public string? ContentTypeHint { get; set; }
}
