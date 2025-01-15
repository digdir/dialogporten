namespace Digdir.Domain.Dialogporten.Domain.Common.DomainEvents;

public sealed class DomainEventContext : IDomainEventContext
{
    private readonly Dictionary<string, string> _metadata = new();

    public Dictionary<string, string> GetMetadata() => _metadata;

    public void AddMetadata(string key, string value) => _metadata[key] = value;
}
