using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Configuration;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Groups;
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
    // TODO: Burde dette være et oppslagsverk? Altså en egen tabell for å lettere kunne søke? kommer selvfølgelig helt an på db tech som brukes.
    public string Party { get; set; } = null!;

    /// <summary>
    /// Vilkårlig referanse som presenteres sluttbruker i UI. Dialogporten tilegger denne ingen semantikk (trenger f.eks. ikke
    /// være unik). Merk at identifikator/primærnøkkel vil kunne være den samme gjennom at tjenestetilbyder kan oppgi "id"
    /// </summary>
    public string? ExternalReference { get; set; } 

    /// <summary>
    /// En vilkårlig streng som er tjenestespesifikk
    /// </summary>
    public string? ExtendedStatus { get; set; }

    // === Dependent relationships ===
    public DialogueStatus.Enum StatusId { get; set; }
    public DialogueStatus Status { get; set; } = null!;

    public long? DialogueGroupId { get; set; }
    public DialogueGroup? DialogueGroup { get; set; } = null!;

    // === Single principal relationships ===
    // TODO: Dette er 1-til-1 relasjon. Noe grunn for å ikke flate ut dersom navnene er gjennomtenkt?
    // eks ContentBody, ContentTitle, ContentSenderName. Jeg har flatet ut actions objektet, se det 
    // for flere eksempler.
    public DialogueContent Content { get; set; } = null!;
    public DialogueDate Dates { get; set; } = null!;
    public DialogueConfiguration Configuration { get; set; } = null!;

    // === Plural principal relationships === 
    public List<DialogueAttachement> Attachments { get; set; } = new();
    public List<DialogueGuiAction> GuiActions { get; set; } = new();
    public List<DialogueApiAction> ApiActions { get; set; } = new();
    public List<DialogueActivity> History { get; set; } = new();
}
