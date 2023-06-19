using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public class Localization : IJoinEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Value { get; set; } = null!;
    public string CultureCode { get; set; } = null!;

    // === Dependent relationships ===
    public Guid LocalizationSetId { get; set; }
    public LocalizationSet LocalizationSet { get; set; } = null!;
}
