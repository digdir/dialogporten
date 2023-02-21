using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public class Localization : IJoinEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public string Value { get; set; } = null!;
    public string CultureCode { get; set; } = null!;

    // === Dependent relationships ===
    public long LocalizationSetId { get; set; }
    public LocalizationSet LocalizationSet { get; set; } = null!;
}
