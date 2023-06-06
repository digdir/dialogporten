using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public class LocalizationSet : IImmutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    // === Plural principal relationships === 
    public List<Localization> Localizations { get; set; } = new();
}
