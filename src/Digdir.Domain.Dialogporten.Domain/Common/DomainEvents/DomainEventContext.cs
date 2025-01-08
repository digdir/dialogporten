using System.Collections.Frozen;

namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public sealed class DomainEventContext : IDomainEventContext
{
    private readonly Dictionary<string, string> _metadata = new();

    public FrozenDictionary<string, string> GetMetadata() => _metadata.ToFrozenDictionary();

    public void AddMetadata(string key, string value) => _metadata[key] = value;

}
