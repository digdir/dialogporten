namespace Digdir.Library.Entity.Abstractions.Features.Versionable;

/// <summary>
/// Provides extension methods for <see cref="IVersionableEntity"/>.
/// </summary>
public static class VersionableExtensions
{
    /// <summary>
    /// Sets properties related to <see cref="IVersionableEntity"/>.
    /// </summary>
    /// <param name="concurrentEntity">The <see cref="IVersionableEntity"/> to update.</param>
    /// <param name="eTag">The entity tag unique to each version of the entity.</param>
    public static void NewVersion(this IVersionableEntity concurrentEntity, Guid? eTag = null) =>
        concurrentEntity.Revision = eTag ?? Guid.NewGuid();
}
