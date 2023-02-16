using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;

public class DialogueActivity : IImutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }


    // TODO: ikke det samme som created by?
    /// <summary>
    /// Indikerer hvem som står bak denne aktiviteten. Fravær av dette feltet indikerer at det er tjenesteilbyder
    /// som har utført aktiviteten.
    /// </summary>
    public string? PerformedBy { get; set; }

    public string? ActivityExtendedType { get; set; }
    public LocalizationSet Description { get; set; } = null!;

    public Uri? DetailsApiUrl { get; set; }
    public Uri? DetailsGuiUrl { get; set; }

    // === Dependent relationships ===
    public DialogueActivityType.Enum TypeId { get; set; }
    public DialogueActivityType Type { get; set; } = null!;

    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}
