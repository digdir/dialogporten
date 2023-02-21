using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public class LocalizationSet : IImutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }

    // === Plural principal relationships === 
    public List<Localization> Localizations { get; set; } = new();
}
