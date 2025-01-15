using System.Collections.ObjectModel;

namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public static class DomainEventMetadataExtensions
{
    public static bool ShouldDisableAltinnEvents(this ReadOnlyDictionary<string, string> metadata)
        => metadata.TryGetValue(Constants.DisableAltinnEvents, out var value) && value == "true";
}
