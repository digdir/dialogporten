using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.SubjectResources;

public class SubjectResource : IEntity
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
