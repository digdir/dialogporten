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

    /// <summary>
    /// Identifikator som refererer en tjenesteressurs ("Altinn Service Resource") i Altinn Autorisasjon
    /// Se https://docs.altinn.studio/technology/solutions/altinn-platform/authorization/resourceregistry/
    /// Dette bestemmer også hvilken autorisasjonspolicy som legges til grunn for både ressursen og tilhørende
    /// </summary>
    public string ServiceResourceIdentifier { get; set; } = null!;

    /// <summary>
    /// Organisasjonsnummer, fødselsnummer eller brukernavn (aka "avgiver" eller "aktør") - altså hvem sin dialogboks 
    /// skal dialogen tilhøre. Brukernavn benyttes for selv-registrerte bruker, og er typisk en e-postadresse.
    /// </summary>
    public string Party { get; set; } = null!;

    /// <summary>
    /// Vilkårlig referanse som presenteres sluttbruker i UI. Dialogporten tilegger denne ingen semantikk (trenger f.eks. ikke
    /// være unik). Merk at identifikator/primærnøkkel vil kunne være den samme gjennom at tjenestetilbyder kan oppgi "id"
    /// </summary>
    public string ExternalReference { get; set; } = null!;

    /// <summary>
    /// En vilkårlig streng som er tjenestespesifikk
    /// </summary>
    public string ExtendedStatus { get; set; } = null!;

    public long ContentId { get; set; }

    public DialogueStatus Status { get; set; } = null!;

    // TODO: Kan dette flates ut? 
    public DialogueDates Dates { get; set; } = null!;
    public DialogueContent Content { get; set; } = null!;
    public DialogueConfiguration Configuration { get; set; } = null!;
    // TODO: Hva er dette konseptet?
    /// <summary>
    /// Alle dialoger som har samme dialoggruppe-id vil kunne grupperes eller på annet vis samles i GUI  
    /// </summary>
    public DialogueGroup DialogueGroup { get; set; } = null!;

    public List<DialogueAttachements> Attachments { get; set; } = new();
    public List<DialogueGuiAction> GuiActions { get; set; } = new();
    public List<DialogueApiAction> ApiActions { get; set; } = new();
    public List<DialogueActivity> History { get; set; } = new();
}
