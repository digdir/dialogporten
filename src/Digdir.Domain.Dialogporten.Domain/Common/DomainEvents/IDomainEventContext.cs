using System.Collections.ObjectModel;

namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public interface IDomainEventContext
{
    ReadOnlyDictionary<string, string> GetMetadata();
    void AddMetadata(string key, string value);
}
