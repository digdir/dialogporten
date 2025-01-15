namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public interface IDomainEventContext
{
    Dictionary<string, string> GetMetadata();
    void AddMetadata(string key, string value);
}
