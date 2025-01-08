using System.Collections.Frozen;

namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public interface IDomainEventContext
{
    FrozenDictionary<string, string> GetMetadata();
    void AddMetadata(string key, string value);
}
