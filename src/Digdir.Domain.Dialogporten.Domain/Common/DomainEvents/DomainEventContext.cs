using System.Collections.ObjectModel;

namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public sealed class DomainEventContext : IDomainEventContext
{
    private readonly Dictionary<string, string> _metadata = new();

    public ReadOnlyDictionary<string, string> GetMetadata() => _metadata.AsReadOnly();

    public void AddMetadata(string key, string value) => _metadata[key] = value;

}
