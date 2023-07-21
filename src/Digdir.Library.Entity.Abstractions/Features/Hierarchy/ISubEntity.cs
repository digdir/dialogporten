namespace Digdir.Library.Entity.Abstractions.Features.Hierarchy;

public interface ISubEntity : IEntityBase
{
    IEntityBase Parent { get; }
}
