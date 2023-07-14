namespace Digdir.Library.Entity.Abstractions.Features.Concurrency;

/// <summary>
/// Provides extension methods for <see cref="IVersionableEntity"/>.
/// </summary>
public static class VersionableExtensions
{
    /// <summary>
    /// Sets properties related to <see cref="IVersionableEntity"/>.
    /// </summary>
    /// <param name="concurrentEntity">The <see cref="IVersionableEntity"/> to update.</param>
    /// <param name="guid">The entity tag unique to each version of the entity.</param>
    public static void NewVersion(this IVersionableEntity concurrentEntity, Guid? guid = null)
    {
        concurrentEntity.ETag = guid ?? Guid.NewGuid();
    }
}