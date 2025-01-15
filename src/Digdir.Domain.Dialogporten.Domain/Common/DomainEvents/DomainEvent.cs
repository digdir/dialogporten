using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;

namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public abstract record DomainEvent : IDomainEvent
{
    [JsonInclude]
    public Guid EventId { get; private set; } = Guid.NewGuid();

    [JsonInclude]
    public DateTimeOffset OccuredAt { get; set; }

    [JsonInclude]
    public ReadOnlyDictionary<string, string> Metadata { get; set; } = new(new Dictionary<string, string>());
}

public static class DomainEventMetadataExtensions
{
    public static bool ShouldDisableAltinnEvents(this ReadOnlyDictionary<string, string> metadata)
        => metadata.TryGetValue(Constants.DisableAltinnEvents, out var value) && value == "true";
}
