using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements.DialogElementUrls;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

public class DialogElement : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Uri? Type { get; set; }
    public LocalizationSet DisplayName { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }

    // === Dependent relationships ===
    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public List<DialogElementUrl> Urls { get; set; } = null!;

    public long? RelatedDialogElementInternalId { get; set; }
    public DialogElement? RelatedDialogElement { get; set; }
}
