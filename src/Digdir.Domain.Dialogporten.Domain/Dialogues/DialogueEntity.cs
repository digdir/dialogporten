using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueEntity : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public string Org { get; set; } = null!;
    public string ServiceResourceIdentifier { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    /// <summary>
    /// Når blir dialogen synlig hos party. Muliggjør opprettelse i forveien og samtidig tilgjengeliggjøring 
    /// for mange parties.
    /// </summary>
    public DateTime VisibleFromUtc { get; set; }
    /// <summary>
    /// Hvis oppgitt blir dialogen satt med en frist 
    /// (i Altinn2 er denne bare retningsgivende og har ingen effekt, skal dette fortsette?)
    /// </summary>
    public DateTime? DueAtUtc { get; set; }
    /// <summary>
    /// Mulighet for å skjule/deaktivere en dialog på et eller annet tidspunkt?
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }

    // === Dependent relationships ===
    public DialogueStatus.Enum StatusId { get; set; }
    public DialogueStatus Status { get; set; } = null!;

    public long BodyId { get; set; }
    public LocalizationSet Body { get; set; } = null!;

    public long TitleId { get; set; }
    public LocalizationSet Title { get; set; } = null!;

    public long SenderNameId { get; set; }
    public LocalizationSet SenderName { get; set; } = null!;

    public long SearchTitleId { get; set; }
    public LocalizationSet SearchTitle { get; set; } = null!;

    // === Principal relationships === 
    public List<DialogueAttachement> Attachments { get; set; } = new();
    public List<DialogueGuiAction> GuiActions { get; set; } = new();
    public List<DialogueApiAction> ApiActions { get; set; } = new();
    public List<DialogueActivity> History { get; set; } = new();
    public List<DialogueTokenScope> TokenScopes { get; set; } = new();
}
