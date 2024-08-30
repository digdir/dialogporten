using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Domain.SubjectResources;

public class SubjectResourceLastUpdate : IIdentifiableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset LastUpdate { get; set; }
}
