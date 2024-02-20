using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Digdir.Library.Entity.Abstractions.Features.Updatable;

namespace Digdir.Library.Entity.Abstractions;

/// <summary>
/// Convenience interface to mark an entity with 
/// <see cref="ICreatableEntity"/>, and 
/// <see cref="IUpdateableEntity"/>.
/// </summary>
/// <remarks>
/// Usually used by entities that joins multiple entities together and thereby has a composite key.
/// </remarks>
public interface IJoinEntity :
    ICreatableEntity,
    IUpdateableEntity;
