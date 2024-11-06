using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.ResourcePolicyMetadata;

public class ResourcePolicyMetadata : IEntity
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = null!;
    public int MinimumSecurityLevel { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
