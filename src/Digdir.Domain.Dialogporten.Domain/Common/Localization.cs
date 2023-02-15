using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Common;

public class Localization : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public string CultureCode { get; set; } = null!;
    public string Value { get; set; } = null!;

    // === Dependent relationships ===
    public long LocalizationSetId { get; set; }
    public LocalizationSet LocalizationSet { get; set; } = null!;

    // TODO: SKal vi ha noe culture oversikt og validering?
    //public Culture Culture { get; set; }
}

public class LocalizationSet : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    // === Plural principal relationships === 
    public List<Localization> Localizations { get; set; } = new();
}
