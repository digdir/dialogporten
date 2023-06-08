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
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    // TODO: Hent dette fra token?
    public string Org { get; set; } = "DummyOrg";
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    /// <summary>
    /// Når blir dialogen synlig hos party. Muliggjør opprettelse i forveien og samtidig tilgjengeliggjøring 
    /// for mange parties.
    /// </summary>
    public DateTimeOffset VisibleFromUtc { get; set; }
    /// <summary>
    /// Hvis oppgitt blir dialogen satt med en frist 
    /// (i Altinn2 er denne bare retningsgivende og har ingen effekt, skal dette fortsette?)
    /// </summary>
    public DateTimeOffset? DueAtUtc { get; set; }
    /// <summary>
    /// Mulighet for å skjule/deaktivere en dialog på et eller annet tidspunkt?
    /// </summary>
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public DateTimeOffset? ReadAtUtc { get; set; }

    // === Dependent relationships ===
    public DialogStatus.Enum StatusId { get; set; }
    public DialogStatus Status { get; set; } = null!;

    public long BodyId { get; set; }
    public LocalizationSet Body { get; set; } = null!;

    public long TitleId { get; set; }
    public LocalizationSet Title { get; set; } = null!;

    public long SenderNameId { get; set; }
    public LocalizationSet SenderName { get; set; } = null!;

    public long SearchTitleId { get; set; }
    public LocalizationSet SearchTitle { get; set; } = null!;

    // === Principal relationships === 
    public List<DialogElement> Elements { get; set; } = new();
    public List<DialogGuiAction> GuiActions { get; set; } = new();
    public List<DialogApiAction> ApiActions { get; set; } = new();
    public List<DialogActivity> History { get; set; } = new();
}
