using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogEntity : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // TODO: Hent dette fra token?
    public string Org { get; set; } = "DummyOrg";
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    // === Dependent relationships ===
    public DialogStatus.Enum StatusId { get; set; }
    public DialogStatus Status { get; set; } = null!;

    public long BodyId { get; set; }
    public LocalizationSet Body { get; set; } = null!;

    public long TitleId { get; set; }
    public LocalizationSet Title { get; set; } = null!;

    public long? SenderNameId { get; set; }
    public LocalizationSet? SenderName { get; set; }

    public long? SearchTitleId { get; set; }
    public LocalizationSet? SearchTitle { get; set; }

    // === Principal relationships === 
    public List<DialogElement> Elements { get; set; } = new();
    public List<DialogGuiAction> GuiActions { get; set; } = new();
    public List<DialogApiAction> ApiActions { get; set; } = new();
    public List<DialogActivity> Activity { get; set; } = new();
}
