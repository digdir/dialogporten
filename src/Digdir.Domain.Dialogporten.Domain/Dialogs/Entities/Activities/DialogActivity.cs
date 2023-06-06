using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivity : IImmutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Indikerer hvem som står bak denne aktiviteten. Fravær av dette feltet indikerer at det er tjenesteilbyder
    /// som har utført aktiviteten.
    /// </summary>
    public string? PerformedBy { get; set; }

    public string? ExtendedType { get; set; }
    public LocalizationSet Description { get; set; } = null!;

    public Uri? DetailsApiUrl { get; set; }
    public Uri? DetailsGuiUrl { get; set; }

    // === Dependent relationships ===
    public DialogActivityType.Enum TypeId { get; set; }
    public DialogActivityType Type { get; set; } = null!;

    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;
}
