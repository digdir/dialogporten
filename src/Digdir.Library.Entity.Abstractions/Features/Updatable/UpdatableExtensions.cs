namespace Digdir.Library.Entity.Abstractions.Features.Updatable;

/// <summary>
/// Provides extension methods for <see cref="IUpdateableEntity"/>.
/// </summary>
public static class UpdatableExtensions
{
    /// <summary>
    /// Sets properties related to <see cref="IUpdateableEntity"/>.
    /// </summary>
    /// <param name="updateable">The <see cref="IUpdateableEntity"/> to update.</param>
    /// <param name="userId">The id of the user that updated this entity.</param>
    /// <param name="utcNow">The update time in UTC.</param>
    public static void Update(this IUpdateableEntity updateable, Guid userId, DateTimeOffset utcNow)
    {
        updateable.UpdatedAtUtc = utcNow;
        updateable.UpdatedByUserId = userId;
    }
}
