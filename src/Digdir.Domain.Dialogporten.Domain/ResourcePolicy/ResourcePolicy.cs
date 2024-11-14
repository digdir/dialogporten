using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.ResourcePolicy;

public sealed class ResourcePolicy : IEntity
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = null!;
    public int MinimumAuthenticationLevel { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
