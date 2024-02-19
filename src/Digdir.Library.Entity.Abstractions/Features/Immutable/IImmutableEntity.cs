using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Digdir.Library.Entity.Abstractions.Features.Updatable;

namespace Digdir.Library.Entity.Abstractions.Features.Immutable;

/// <summary>
/// Convenience interface to mark an entity with 
/// <see cref="IIdentifiableEntity"/>, and
/// <see cref="ICreatableEntity"/>.
/// </summary>
/// <remarks>
/// Differs from <see cref="IEntity"/> by not 
/// including <see cref="IUpdateableEntity"/>.
/// </remarks>
public interface IImmutableEntity :
    IIdentifiableEntity,
    ICreatableEntity;