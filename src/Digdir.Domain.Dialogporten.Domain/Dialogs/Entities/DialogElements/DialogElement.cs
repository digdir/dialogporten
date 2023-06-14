using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

public class DialogElement : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public string? AuthorizationAttribute { get; set; }
    public Uri? Type { get; set; }

    // === Dependent relationships ===
    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public long DisplayNameId { get; set; }
    public LocalizationSet DisplayName { get; set; } = null!;

    public long? RelatedDialogElementId { get; set; }
    public DialogElement? RelatedDialogElement { get; set; }

    // === Principal relationships ===
    public List<DialogElementUrl> Urls { get; set; } = new();
    public List<DialogApiAction> ApiActions { get; set; } = new();
    public List<DialogActivity> Activities { get; set; } = new();
    public List<DialogElement> RelatedDialogElements { get; set; } = new();
}
