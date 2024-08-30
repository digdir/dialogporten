using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Updatable;

namespace Digdir.Domain.Dialogporten.Domain.SubjectResources;

public class SubjectResource : IEntity
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
