using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Concurrency;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogEntity : IEntity, ISoftDeletableEntity, IVersionableEntity
{
    public Guid Id { get; set; }
    public Guid ETag { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool Deleted { get; set; }
    public bool HardDelete { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // TODO: Hent dette fra token?
    public string Org { get; set; } = "DummyOrg";
    public Uri ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    // === Dependent relationships ===
    public DialogStatus.Enum StatusId { get; set; }
    public DialogStatus Status { get; set; } = null!;

    public Guid BodyId { get; set; }
    public LocalizationSet Body { get; set; } = null!;

    public Guid TitleId { get; set; }
    public LocalizationSet Title { get; set; } = null!;

    public Guid SenderNameId { get; set; }
    public LocalizationSet SenderName { get; set; } = null!;

    public Guid SearchTitleId { get; set; }
    public LocalizationSet SearchTitle { get; set; } = null!;

    // === Principal relationships === 
    public List<DialogElement> Elements { get; set; } = new();
    public List<DialogGuiAction> GuiActions { get; set; } = new();
    public List<DialogApiAction> ApiActions { get; set; } = new();
    public List<DialogActivity> Activities { get; set; } = new();
}
