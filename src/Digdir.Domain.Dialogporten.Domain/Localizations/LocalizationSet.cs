using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public abstract class LocalizationSet : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // === Plural principal relationships === 
    [AggregateChild]
    public List<Localization> Localizations { get; set; } = [];
}
